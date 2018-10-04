using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using PPNetLib;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using PPServer.Database;
using ProtoBuf;
using PPNetLib.Tcp;
using PPNetLib.Contracts.Monitor;

namespace PPServer
{
    public class Server
    {
        /// <summary>
        /// Local data transfer object
        /// that holds all the zones
        /// </summary>
        public static Dto2Object Dto;

        /// <summary>
        /// The maximum number of zones to be
        /// loaded into the memory
        /// </summary>
        public int MaxZones;

        /// <summary>
        /// Guard object used to ban IPs on
        /// bad requests
        /// </summary>
        public readonly Guard Guard;

        /// <summary>
        /// IP address of the server to listen on
        /// </summary>
        private readonly string _ip;

        /// <summary>
        /// Holds the port number the server will use
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// Used to handle incoming requests
        /// </summary>
        private readonly Handler _handler;

        /// <summary>
        /// Holds the values of <see cref="ConsoleKit.MessageType"/>.
        /// Used to differentiate between error message types
        /// </summary>
        private readonly Array _messageTypes;

        /// <summary>
        /// Object for the underlying Tcp server implementation
        /// </summary>
        private WatsonTcpServer _watsonTcpServer;

        /// <summary>
        /// Object for the underlying Http server implementation
        /// </summary>
        private Http.Handler _httpHandler;

        /// <summary>
        /// Used to catch Console.WriteLine events
        /// </summary>
        private ConsoleWriter _writer;

        /// <summary>
        /// Holds the type of the last error message
        /// </summary>
        private string _messageHeap;

        /// <summary>
        /// Ctor.
        /// Load defaults from the configuraion file,
        /// set up needed objects
        /// </summary>
        public Server()
        {
            var configSect = ConfigurationManager.GetSection("ServerConfiguration") as NameValueCollection;

            // ReSharper disable once PossibleNullReferenceException
            var ip = configSect["IPAddress"];
            if (ip == null)
            {
                _ip = Extensions.Net.GetLocalIPv4(NetworkInterfaceType.Ethernet);
                if (_ip == string.Empty)
                    _ip = Extensions.Net.GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            }
            else
            {
                _ip = configSect["IPAddress"];
            }

            _port = int.Parse(configSect["Port"]);
            _handler = new Handler(this);
            Guard = new Guard(this, 5);

            MaxZones = 0;

            _messageTypes = Enum.GetValues(typeof(ConsoleKit.MessageType));
        }

        /// <summary>
        /// If a consolewriter is set using SetWriter() this method will be
        /// invoked on each Console.Write event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Writer_WriteEvent(object sender, ConsoleWriterEventArgs e)
        {
            BroadcastMonitorAck(e.Value);
        }

        /// <summary>
        /// If a consolewriter is set using SetWriter() this method will be
        /// invoked on each Console.WriteLine event
        /// </summary>
        private void Writer_WriteLineEvent(object sender, ConsoleWriterEventArgs e)
        {
            BroadcastMonitorAck(e.Value);
        }

        /// <summary>
        /// Load data from the database
        /// </summary>
        public void LoadData()
        {
            Dto = new Dto2Object() {
                Zones = new List<PolyZone>()
            };

            int count = 0;
            if (MaxZones > 0)
                count = MaxZones;
            else
                count = Sql.Instance.GetZoneCount();

            var current = 0;
            Sql.Instance.LoadZones((zone) => {
                Dto.Zones.Add(zone);
                current++;

                ConsoleKit.Message(ConsoleKit.MessageType.INFO, "Loading {0}/{1} zones\t\r", current, count);
                return true;
            }, count);
            Console.Write("\n");
        }

        /// <summary>
        /// Set up the Http server
        /// </summary>
        public void SetupHttpServer()
        {
            _httpHandler = new Http.Handler(this);
            _httpHandler.Handle();
        }

        /// <summary>
        /// Start listening on the specified ip:port
        /// </summary>
        public void Listen()
        {
            ConsoleKit.Message(ConsoleKit.MessageType.INFO, "PP TCP Server starting up, protocol version {0}\n", Protocol.Version);

#if DEBUG
            _watsonTcpServer = new WatsonTcpServer(_ip, _port, ClientConnected, ClientDisconnected, MessageReceived, true);
#else
            _watsonTcpServer = new WatsonTcpServer(_ip, _port, ClientConnected, ClientDisconnected, MessageReceived, false);
#endif

            ConsoleKit.Message(ConsoleKit.MessageType.INFO, $"PP TCP server listening on {_ip}:{_port}\n");
        }

        /// <summary>
        /// Invoked whenever a client connects to the server
        /// </summary>
        /// <param name="ipPort"></param>
        /// <returns></returns>
        private bool ClientConnected(string ipPort)
        {
            ConsoleKit.Message(ConsoleKit.MessageType.INFO, "Client connected: {0}\n", ipPort);
            return true;
        }

        /// <summary>
        /// Invoked whenever a client disconnects from the server
        /// </summary>
        /// <param name="ipPort"></param>
        /// <returns></returns>
        private bool ClientDisconnected(string ipPort)
        {
            var user = Guard.GetAuthUser(ipPort);
            Guard.RemoveAuthUser(user);

            ConsoleKit.Message(ConsoleKit.MessageType.INFO, "Client disconnected: {0}\n", ipPort);
            BroadcastOnlineUsersAck();
            return true;
        }

        /// <summary>
        /// Processing and deserialization of incoming messages
        /// </summary>
        /// <param name="ipPort">IP:Port of the sender client</param>
        /// <param name="data">Byte array that contains data</param>
        /// <returns></returns>
        private bool MessageReceived(string ipPort, byte[] data)
        {
            if (data == null || data.Length <= 0) return false;

            try
            {
                var ipOnly = ipPort.Split(':')[0];
                var user = Guard.GetAuthUser(ipPort);

                // If the client is banned, drop the cconnection
                if(Guard.IsBanned(ipOnly) && !Guard.CheckExpired(ipOnly))
                {
                    throw new Exception($"Refused to process requests from {ipOnly}. Ip banned!");
                }

                using (var stream = new MemoryStream(data))
                {
                    // Check protocol version
                    // TODO: MessageReceived should not even get called if the protocol dont't match
                    var bProtocolVersion = new byte[4];
                    stream.Read(bProtocolVersion, 0, 4);
                    var protocolVersion = BitConverter.ToInt32(bProtocolVersion, 0);
                    if(protocolVersion != Protocol.Version)
                        throw new Exception($"Invalid protocol version {ipPort}");

                    // Fetch packet id
                    var bPacketId = new byte[4];
                    stream.Read(bPacketId, 0, 4);
                    var packetId = (Protocols)BitConverter.ToInt32(bPacketId, 0);

                    ConsoleKit.Message(ConsoleKit.MessageType.INFO, "PID {0} received from {1}\n", Enum.GetName(typeof(Protocols), packetId), ipPort);

                    // ReSharper disable once SwitchStatementMissingSomeCases
                    // Process each message type
                    switch (packetId)
                    {
                        case Protocols.LOGIN_REQ:
                            var loginReq = Serializer.Deserialize<LoginReq>(stream);
                            User loginUser;

                            if(user != null)
                            {
                                Send(ipPort, new LoginDuplicateAck());
                            }
                            else
                            {
                                loginUser = _handler.OnLoginReq(loginReq, ipPort);

                                if (loginUser != null)
                                {
                                    Guard.AddAuthUser(loginUser);
                                    BroadcastOnlineUsersAck();
                                }
                                else
                                {
                                    Guard.TryCheck(ipOnly);
                                }
                            }
                            
                            break;

                        case Protocols.ZONECOUNT_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Guest))
                                goto default;

                            _handler.OnZoneCountReq(ipPort);
                            break;

                        case Protocols.ZONELIST_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Guest))
                                goto default;
                            _handler.OnZoneListReq(ipPort);
                            break;

                        case Protocols.INSERTZONE_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Editor))
                                goto default;

                            var insertZoneReq = Serializer.Deserialize<InsertZoneReq>(stream);
                            _handler.OnInsertZoneReqAsync(insertZoneReq, user);
                            break;

                       case Protocols.REMOVEZONE_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Editor))
                                goto default;

                            var removeZoneReq = Serializer.Deserialize<RemoveZoneReq>(stream);
                            _handler.OnRemoveZoneReq(removeZoneReq, user);
                            break;

                        case Protocols.UPDATEZONE_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Editor))
                                goto default;

                            var updateZoneReq = Serializer.Deserialize<UpdateZoneReq>(stream);
                            _handler.OnUpdateZoneReq(updateZoneReq, user);
                            break;

                        case Protocols.REMOVEPOINT_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Editor))
                                goto default;

                            var removePointReq = Serializer.Deserialize<RemovePointReq>(stream);
                            _handler.OnRemovePointReq(removePointReq, user);
                            break;

                        case Protocols.INSERTPOINT_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Editor))
                                goto default;

                            var insertPointReq = Serializer.Deserialize<InsertPointReq>(stream);
                            _handler.OnInsertPointReqAsync(insertPointReq, user);
                            break;

                        case Protocols.UPDATEPOINT_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Editor))
                                goto default;

                            var updatePointReq = Serializer.Deserialize<UpdatePointReq>(stream);
                            _handler.OnUpdatePointReq(updatePointReq, user);
                            break;

                        case Protocols.CITYLIST_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Editor))
                                goto default;

                            _handler.OnCityListReqAsync(user);
                            break;

                        case Protocols.USERLIST_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            _handler.OnUserListReqAsync(user);
                            break;

                        case Protocols.INSERTUSER_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            var insertUserReq = Serializer.Deserialize<InsertUserReq>(stream);
                            _handler.OnInsertUserReq(insertUserReq);
                            break;

                        case Protocols.REMOVEUSER_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            var removeUserReq = Serializer.Deserialize<RemoveUserReq>(stream);
                            _handler.OnRemoveUserReq(removeUserReq);
                            break;

                        case Protocols.UPDATEUSER_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            var updateUserReq = Serializer.Deserialize<UpdateUserReq>(stream);
                            _handler.OnUpdateUserReq(updateUserReq);
                            break;

                        case Protocols.ISDUPLICATEUSER_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            var isDuplicateUserReq = Serializer.Deserialize<IsDuplicateUserReq>(stream);
                            _handler.OnIsDuplicateUserReq(isDuplicateUserReq, user);
                            break;

                        case Protocols.ONLINEUSERS_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;
                            _handler.OnOnlineUsersReq(user);
                            break;

                        case Protocols.DISCONNECTUSER_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            var disconnectUserReq = Serializer.Deserialize<DisconnectUserReq>(stream);
                            _handler.OnDisconnectUserReq(disconnectUserReq);
                            break;

                        case Protocols.BANIPADDRESS_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            var banIpAddressReq = Serializer.Deserialize<BanIpAddressReq>(stream);
                            _handler.OnBanIPAddressReq(banIpAddressReq, user);
                            break;

                        case Protocols.LISTBANNEDIPS_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            _handler.OnListBannedIPsReq(user);
                            break;

                        case Protocols.UNBANIPADDRESS_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            var unbanIpAddressReq = Serializer.Deserialize<UnbanIPAddressReq>(stream);
                            _handler.OnUnbanIPAddressReq(unbanIpAddressReq, user);
                            break;

                        case Protocols.COMMAND_REQ:
                            if (!Guard.CheckPrivileges(user, GroupRole.Admin))
                                goto default;

                            var commandReq = Serializer.Deserialize<CommandReq>(stream);
                            _handler.OnCommandReq(commandReq, user);
                            break;

                        default:
                            // Unknown or invalid message.
                            // Increase tries and disconnecct the client
                            Guard.TryCheck(ipOnly);
                            _watsonTcpServer.DisconnectClient(ipPort);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                // Print out the exception and disconnect the client that might have caused it
                ConsoleKit.Message(ConsoleKit.MessageType.ERROR, e.Message + "\n" + e.StackTrace + "\n");
                _watsonTcpServer.DisconnectClient(ipPort);
            }

            return true;
        }

        /// <summary>
        /// Disconnect an user from the server
        /// </summary>
        /// <param name="userId"></param>
        public void DisconnectUser(int userId)
        {
            var user = Guard.GetAuthUser(userId);
            if(user != null)
            {
                Send(user.IpPort, new AbortSessionAck());
                Guard.RemoveAuthUser(user);
                _watsonTcpServer.DisconnectClient(user.IpPort);
            }
            
        }

        // TODO: Be able to DC not logged in users
        /// <summary>
        /// Disconnect a client from the seerver
        /// </summary>
        /// <param name="ipPort"></param>
        public void DisconnectUser(string ipPort)
        {
            var user = Guard.GetAuthUser(ipPort);
            if (user != null)
            {
                Send(user.IpPort, new AbortSessionAck());
                Guard.RemoveAuthUser(user);
                _watsonTcpServer.DisconnectClient(user.IpPort);
            }
        }

        /// <summary>
        /// Send a packet to every connected client but one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet">The packet to be sent</param>
        /// <param name="except">IP address of the exempt client</param>
        public void SendToEveryoneExcept<T>(T packet, string except) where T: Packet
        {
            var clients = _watsonTcpServer.ListClients();
            foreach(var client in clients)
                if(except != client)
                    Send(client, packet);
        }

        /// <summary>
        /// Send a packet to everyone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet"></param>
        public void SendToEveryone<T>(T packet) where T: Packet
        {
            var clients = _watsonTcpServer.ListClients();
            foreach (var client in clients)
                Send(client, packet);
        }

        /// <summary>
        /// Send a packet to a logged in user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="user"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool Send<T>(User user, T packet) where T: Packet
        {
            return Send(user.IpPort, packet);
        }

        /// <summary>
        /// Send a packet to a client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ipPort"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool Send<T>(string ipPort, T packet) where T: Packet
        {
            var packetId = (int)packet.PacketId;

            using (var stream = new MemoryStream())
            {
                var protocolVersion = BitConverter.GetBytes(Protocol.Version);
                stream.Write(protocolVersion, 0, 4);

                var pid = BitConverter.GetBytes(packetId);
                stream.Write(pid, 0, 4);
                Serializer.Serialize(stream, packet);

                var buffer = stream.ToArray();

                return _watsonTcpServer.Send(ipPort, buffer);
                //ConsoleKit.Message(ConsoleKit.MessageType.INFO, "PID {0} of {1} bytes sent to {2}\n", Enum.GetName(typeof(Protocols), packetId), buffer.Length, ipPort);
            }
        }

        /// <summary>
        /// Returns a list of all the connected clients
        /// </summary>
        /// <returns></returns>
        public List<string> GetClients()
        {
            return _watsonTcpServer.ListClients();
        }

        // TODO: Announce only
        /// <summary>
        /// Notify every client that the server will
        /// shut down in <paramref name="seconds"/> seconds
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="shutdown"></param>
        public async void AnnounceShutdownAck(int seconds, bool shutdown = true)
        {
            SendToEveryone(new ShutdownAck() { Seconds = seconds });
            await Task.Delay(seconds * 1000);
            if(shutdown)
                Shutdown();
        }

        /// <summary>
        /// Shuts down the server
        /// </summary>
        public void Shutdown()
        {
            var clients = _watsonTcpServer.ListClients();
            foreach (var client in clients)
                _watsonTcpServer.DisconnectClient(client);
            _watsonTcpServer.Dispose();
            Environment.Exit(0);
        }

        /// <summary>
        /// Broadcast an error message to all
        /// the connected clients with the Monitor
        /// flag enabled
        /// </summary>
        /// <param name="message"></param>
        private void BroadcastMonitorAck(string message)
        {
            if (_watsonTcpServer != null)
            {
                foreach (var type in _messageTypes)
                {
                    var messageType = $"[{type}]";
                    if (message.IndexOf(messageType) == 0)
                    {
                        _messageHeap = messageType;
                        return;
                    }
                }

                if(_messageHeap != string.Empty)
                {
                    message = _messageHeap + message;
                    _messageHeap = string.Empty;
                }

                var users = Guard.GetAuthUsers();
                foreach (var user in users)
                {
                    if (user.Monitor && _watsonTcpServer.IsClientConnected(user.IpPort))
                    {
                        Send(user, new ServerMonitorAck() { Output = message });
                    }
                }
            }
        }

        /// <summary>
        /// Send a list of online users to all the connected clients with the Monitor flag enabled
        /// </summary>
        private void BroadcastOnlineUsersAck()
        {
            var users = Guard.GetAuthUsers();
            foreach (var user in users)
            {
                if (user.Monitor && _watsonTcpServer.IsClientConnected(user.IpPort))
                {
                    Send(user, new OnlineUsersAck() { OnlineUsersList = users });
                }
            }
        }

        /// <summary>
        /// Limit the count of zones to be loaded into the memory
        /// to speed up debugging
        /// </summary>
        /// <param name="count"></param>
        public void LimitZones(int count)
        {
            MaxZones = count;
        }

        /// <summary>
        /// Redirect console output
        /// </summary>
        /// <param name="writer"></param>
        public void SetWriter(ConsoleWriter writer)
        {
            if(_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }

            writer.WriteLineEvent += Writer_WriteLineEvent;
            writer.WriteEvent += Writer_WriteEvent;
            _writer = writer;
        }
    }
}
