using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using PPNetLib;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using PPServer.Database;
using ProtoBuf;
using watsontcp_dotnetcore.Tcp;

namespace PPServer
{
    public class Server
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly Handler _handler;
        private WatsonTcpServer _watsonTcpServer;
        private readonly Dictionary<string, User> _authUsers;
        private Http.Handler _httpHandler;
        public Dto2Object Dto;

        public Server(bool useHTTP = true)
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
            _authUsers = new Dictionary<string, User>();
            PrintAsciiArtLogo();

            LoadData();

            if (useHTTP)
            {
                _httpHandler = new Http.Handler();
                _httpHandler.Handle();
            }
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
            _authUsers.Remove(ipPort);
            ConsoleKit.Message(ConsoleKit.MessageType.INFO, "Client disconnected: {0}\n", ipPort);
            return true;
        }

        private bool MessageReceived(string ipPort, byte[] data)
        {
            if (data == null || data.Length <= 0) return false;

            try
            {
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
                            var user = _handler.OnLoginReq(loginReq, ipPort, _authUsers);

                            if (user != null)
                            {
                                lock(_authUsers)
                                {
                                    if (!_authUsers.ContainsKey(ipPort))
                                        _authUsers.Add(ipPort, user);
                                }
                            }
                            break;

                        case Protocols.ZONECOUNT_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Guest))
                                goto default;

                            _handler.OnZoneCountReq(ipPort);
                            break;

                        case Protocols.ZONELIST_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Guest))
                                goto default;
                            _handler.OnZoneListReq(ipPort);
                            break;

                        case Protocols.INSERTZONE_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Editor))
                                goto default;

                            var insertZoneReq = Serializer.Deserialize<InsertZoneReq>(stream);
                            _handler.OnInsertZoneReqAsync(insertZoneReq, ipPort);
                            break;

                       case Protocols.REMOVEZONE_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Editor))
                                goto default;

                            var removeZoneReq = Serializer.Deserialize<RemoveZoneReq>(stream);
                            _handler.OnRemoveZoneReq(removeZoneReq, ipPort);
                            break;

                        case Protocols.UPDATEZONE_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Editor))
                                goto default;

                            var updateZoneReq = Serializer.Deserialize<UpdateZoneReq>(stream);
                            _handler.OnUpdateZoneReq(updateZoneReq, ipPort);
                            break;

                        case Protocols.REMOVEPOINT_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Editor))
                                goto default;

                            var removePointReq = Serializer.Deserialize<RemovePointReq>(stream);
                            _handler.OnRemovePointReq(removePointReq, ipPort);
                            break;

                        case Protocols.INSERTPOINT_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Editor))
                                goto default;

                            var insertPointReq = Serializer.Deserialize<InsertPointReq>(stream);
                            _handler.OnInsertPointReqAsync(insertPointReq, ipPort);
                            break;

                        case Protocols.UPDATEPOINT_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Editor))
                                goto default;

                            var updatePointReq = Serializer.Deserialize<UpdatePointReq>(stream);
                            _handler.OnUpdatePointReq(updatePointReq, ipPort);
                            break;

                        case Protocols.CITYLIST_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Editor))
                                goto default;

                            _handler.OnCityListReqAsync(ipPort);
                            break;

                        case Protocols.USERLIST_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Admin))
                                goto default;

                            _handler.OnUserListReqAsync(ipPort);
                            break;

                        case Protocols.INSERTUSER_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Admin))
                                goto default;

                            var insertUserReq = Serializer.Deserialize<InsertUserReq>(stream);
                            _handler.OnInsertUserReq(insertUserReq);
                            break;

                        case Protocols.REMOVEUSER_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Admin))
                                goto default;

                            var removeUserReq = Serializer.Deserialize<RemoveUserReq>(stream);
                            _handler.OnRemoveUserReq(removeUserReq);
                            break;

                        case Protocols.UPDATEUSER_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Admin))
                                goto default;

                            var updateUserReq = Serializer.Deserialize<UpdateUserReq>(stream);
                            _handler.OnUpdateUserReq(updateUserReq);
                            break;

                        case Protocols.ISDUPLICATEUSER_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Admin))
                                goto default;

                            var isDuplicateUserReq = Serializer.Deserialize<IsDuplicateUserReq>(stream);
                            _handler.IsDuplicateUserReq(isDuplicateUserReq, ipPort);
                            break;

                        default:
                            _watsonTcpServer.DisconnectClient(ipPort);
                            ConsoleKit.Message(ConsoleKit.MessageType.ERROR, "Invalid message from {0}\n", ipPort);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleKit.Message(ConsoleKit.MessageType.ERROR, e.Message);
                _watsonTcpServer.DisconnectClient(ipPort);
            }

            return true;
        }

        public void DisconnectUser(int userId)
        {
            var user = _authUsers.FirstOrDefault(u => u.Value.Id == userId);
            if(user.Key != null)
            _watsonTcpServer.DisconnectClient(user.Key);
        }

        public void SendToEveryoneExcept<T>(T packet, string except)
        {
            var clients = _watsonTcpServer.ListClients();
            foreach(var client in clients)
                if(except != client)
                    Send(client, packet);
        }

        public void SendToEveryone<T>(T packet)
        {
            var clients = _watsonTcpServer.ListClients();
            foreach (var client in clients)
                Send(client, packet);
        }

        public void Send<T>(string ipPort, T packet)
        {
            var packetId = (int)((Packet)(object)packet).PacketId;

            using (var stream = new MemoryStream())
            {
                var protocolVersion = BitConverter.GetBytes(Protocol.Version);
                stream.Write(protocolVersion, 0, 4);

                var pid = BitConverter.GetBytes(packetId);
                stream.Write(pid, 0, 4);
                Serializer.Serialize(stream, packet);

                var buffer = stream.ToArray();

                _watsonTcpServer.Send(ipPort, buffer);
                //ConsoleKit.Message(ConsoleKit.MessageType.INFO, "PID {0} of {1} bytes sent to {2}\n", Enum.GetName(typeof(Protocols), packetId), buffer.Length, ipPort);
            }
        }

        private bool CheckPrivileges(string ipPort, GroupRole min)
        {
            lock(_authUsers)
            {
                if (_authUsers.ContainsKey(ipPort) && _authUsers[ipPort] != null)
                {
                    return _authUsers[ipPort].GroupRole >= min;
                }
            }
            return false;
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
    }
}
