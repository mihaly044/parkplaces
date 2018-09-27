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
        public Dto2Object Dto;
        public readonly Guard Guard;

        private readonly string _ip;
        private readonly int _port;
        private readonly Handler _handler;
        private readonly Array _messageTypes;

        private WatsonTcpServer _watsonTcpServer;
        private Http.Handler _httpHandler;
        private string _messageHeap;  

        public Server(ConsoleWriter writer, bool useHttp = true)
        {
            writer.WriteLineEvent += Writer_WriteLineEvent;
            writer.WriteEvent += Writer_WriteEvent;

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
            PrintAsciiArtLogo();

            LoadData();

            if (useHttp)
            {
                _httpHandler = new Http.Handler(this);
                _httpHandler.Handle();
            }

            _messageTypes = Enum.GetValues(typeof(ConsoleKit.MessageType));
        }

        private void Writer_WriteEvent(object sender, ConsoleWriterEventArgs e)
        {
            BroadcastMonitorAck(e.Value);
        }

        private void Writer_WriteLineEvent(object sender, ConsoleWriterEventArgs e)
        {
            BroadcastMonitorAck(e.Value);
        }

        private void LoadData()
        {
            Dto = new Dto2Object() {
                Zones = new List<PolyZone>()
            };

            var count = Sql.Instance.GetZoneCount();
            var current = 0;

            Sql.Instance.LoadZones((zone) => {
                Dto.Zones.Add(zone);
                current++;

                ConsoleKit.Message(ConsoleKit.MessageType.INFO, "Loading {0}/{1} zones\t\r", current, count);
                return true;
            });
            Console.Write("\n");
        }

        private void PrintAsciiArtLogo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                @"
8888888b.  8888888b.   .d8888b.                                             
888   Y88b 888   Y88b d88P  Y88b                                            
888    888 888    888 Y88b.                                                 
888   d88P 888   d88P  ""Y888b.    .d88b.  888d888 888  888  .d88b.  888d888 
8888888P""  8888888P""      ""Y88b. d8P  Y8b 888P""   888  888 d8P  Y8b 888P""   
888        888              ""888 88888888 888     Y88  88P 88888888 888     
888        888        Y88b  d88P Y8b.     888      Y8bd8P  Y8b.     888     
888        888         ""Y8888P""   ""Y8888  888       Y88P    ""Y8888  888
");
            Console.ResetColor();
        }

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

        private bool ClientConnected(string ipPort)
        {
            ConsoleKit.Message(ConsoleKit.MessageType.INFO, "Client connected: {0}\n", ipPort);
            return true;
        }

        private bool ClientDisconnected(string ipPort)
        {
            var user = Guard.IP2User(ipPort);
            Guard.RemoveAuthUser(user);

            ConsoleKit.Message(ConsoleKit.MessageType.INFO, "Client disconnected: {0}\n", ipPort);
            BroadcastOnlineUsersAck();
            return true;
        }

        private bool MessageReceived(string ipPort, byte[] data)
        {
            if (data == null || data.Length <= 0) return false;

            try
            {
                var ipOnly = ipPort.Split(':')[0];
                var user = Guard.IP2User(ipPort);

                if(Guard.IsBanned(ipOnly) && !Guard.CheckExpired(ipOnly))
                {
                    throw new Exception($"Refused to communicate with {ipOnly}. Ip banned!");
                }

                using (var stream = new MemoryStream(data))
                {
                    var bProtocolVersion = new byte[4];
                    stream.Read(bProtocolVersion, 0, 4);
                    var protocolVersion = BitConverter.ToInt32(bProtocolVersion, 0);
                    if(protocolVersion != Protocol.Version)
                        throw new Exception($"Invalid protocol version {ipPort}");

                    var bPacketId = new byte[4];
                    stream.Read(bPacketId, 0, 4);
                    var packetId = (Protocols)BitConverter.ToInt32(bPacketId, 0);

                    ConsoleKit.Message(ConsoleKit.MessageType.INFO, "PID {0} received from {1}\n", Enum.GetName(typeof(Protocols), packetId), ipPort);

                    // ReSharper disable once SwitchStatementMissingSomeCases
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
                            _handler.OnBanIPAddressReq(banIpAddressReq);
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
                            _handler.OnUnbanIPAddressReq(unbanIpAddressReq);
                            break;

                        default:
                            Guard.TryCheck(ipOnly);
                            _watsonTcpServer.DisconnectClient(ipPort);
                            throw new Exception("Invalid message from {0}\n");
                    }
                }
            }
            catch (Exception e)
            {
                throw;
                ConsoleKit.Message(ConsoleKit.MessageType.ERROR, e.Message + "\n" + e.StackTrace + "\n");
                _watsonTcpServer.DisconnectClient(ipPort);
            }

            return true;
        }

        public void DisconnectUser(int userId)
        {
            var user = Guard.GetAuthUser(userId);
            if(user != null)
            {
                Send(user.IpPort, new AbortSessionAck());
                _watsonTcpServer.DisconnectClient(user.IpPort);
            }
            
        }

        public void DisconnectUser(string ipPort)
        {
            Send(ipPort, new AbortSessionAck());
            _watsonTcpServer.DisconnectClient(ipPort);
        }

        public void SendToEveryoneExcept<T>(T packet, string except) where T: Packet
        {
            var clients = _watsonTcpServer.ListClients();
            foreach(var client in clients)
                if(except != client)
                    Send(client, packet);
        }

        public void SendToEveryone<T>(T packet) where T: Packet
        {
            var clients = _watsonTcpServer.ListClients();
            foreach (var client in clients)
                Send(client, packet);
        }

        public bool Send<T>(User user, T packet) where T: Packet
        {
            return Send(user.IpPort, packet);
        }

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

        public List<string> GetClients()
        {
            return _watsonTcpServer.ListClients();
        }

        public async void AnnounceShutdownAck(int seconds, bool shutdown = true)
        {
            SendToEveryone(new ShutdownAck() { Seconds = seconds });
            await Task.Delay(seconds * 1000);
            if(shutdown)
                Shutdown();
        }

        public void Shutdown()
        {
            var clients = _watsonTcpServer.ListClients();
            foreach (var client in clients)
                _watsonTcpServer.DisconnectClient(client);
            _watsonTcpServer.Dispose();
            Environment.Exit(0);
        }

        private void BroadcastMonitorAck(string message)
        {
            if (_watsonTcpServer != null)
            {
                foreach (var type in _messageTypes)
                {
                    if(message.IndexOf($"[{type}]") == 0)
                    {
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
                    if (user.Monitor)
                    {
                        Send(user, new ServerMonitorAck() { Output = message });
                    }
                }
            }
        }

        private void BroadcastOnlineUsersAck()
        {
            var users = Guard.GetAuthUsers();
            foreach (var user in users)
            {
                if (user.Monitor)
                {
                    Send(user, new OnlineUsersAck() { OnlineUsersList = users });
                }
            }
        }
    }
}
