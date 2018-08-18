using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPNetLib;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using ProtoBuf;
using WatsonTcp;

namespace PPServer
{
    public class Server
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly Handler _handler;
        private WatsonTcpServer _watsonTcpServer;
        private readonly Dictionary<string, User> _authUsers;

        public Server()
        {
            var configSect = ConfigurationManager.GetSection("ServerConfiguration") as NameValueCollection;
            // ReSharper disable once PossibleNullReferenceException
            _ip = configSect["IPAddress"];
            _port = int.Parse(configSect["Port"]);

            _handler = new Handler(this);
            _authUsers = new Dictionary<string, User>();
        }

        public void Listen()
        {
            _watsonTcpServer = new WatsonTcpServer(_ip, _port, ClientConnected, ClientDisconnected, MessageReceived, true);
        }

        private bool ClientConnected(string ipPort)
        {
            Console.WriteLine("Client connected: " + ipPort);
            return true;
        }

        private bool ClientDisconnected(string ipPort)
        {
            _authUsers.Remove(ipPort);
            Console.WriteLine("Client disconnected: " + ipPort);
            return true;
        }

        private bool MessageReceived(string ipPort, byte[] data)
        {
            if (data == null || data.Length <= 0) return false;

            try
            {
                using (var stream = new MemoryStream(data))
                {
                    var bPacketId = new byte[4];
                    stream.Read(bPacketId, 0, 4);
                    var packetId = (Protocols)BitConverter.ToInt32(bPacketId, 0);

                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (packetId)
                    {
                        case Protocols.LOGIN_REQ:
                            var loginReq = Serializer.Deserialize<LoginReq>(stream);
                            var user = _handler.OnLoginReq(loginReq, ipPort);

                            if(!_authUsers.ContainsKey(ipPort))
                                _authUsers.Add(ipPort, user);
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
                            _handler.OnRemoveZoneReq(removeZoneReq);
                            break;

                        case Protocols.REMOVEPOINT_REQ:
                            if (!CheckPrivileges(ipPort, GroupRole.Editor))
                                goto default;

                            var removePointReq = Serializer.Deserialize<RemovePointReq>(stream);
                            _handler.OnRemovePointReq(removePointReq);
                            break;

                        default:
                            _watsonTcpServer.DisconnectClient(ipPort);
                            break;
                    }
                    Console.WriteLine("Received PID {0} from {1}", packetId, ipPort);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return true;
        }

        public void Send<T>(string ipPort, T packet)
        {
            var packetId = (int)((Packet)(object)packet).PacketID;

            using (var stream = new MemoryStream())
            {
                var pid = BitConverter.GetBytes(packetId);
                stream.Write(pid, 0, 4);
                Serializer.Serialize(stream, packet);

                var buffer = stream.ToArray();

                _watsonTcpServer.Send(ipPort, buffer);
                //Console.WriteLine("PID {0} of {1} bytes sent to {2}", PacketID, buffer.Length, ipPort);
            }
        }

        private bool CheckPrivileges(string ipPort, GroupRole min)
        {
            if(_authUsers.ContainsKey(ipPort) && _authUsers[ipPort] != null)
            {
                return _authUsers[ipPort].GroupRole >= min;
            }
            return false;
        }
    }
}
