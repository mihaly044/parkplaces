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
        private string _ip;
        private int _port;
        private Handler _handler;
        private WatsonTcpServer _watsonTcpServer;
        private Dictionary<string, User> _authUsers;

        public Server()
        {
            var configSect = ConfigurationManager.GetSection("ServerConfiguration") as NameValueCollection;
            try
            {
                _ip = configSect["IPAddress"];
                _port = Int32.Parse(configSect["Port"]);
            }
            catch (Exception e)
            {
                throw e;
            }
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
            if (data != null && data.Length > 0)
            {
                try
                {
                    using (var stream = new MemoryStream(data))
                    {
                        var bPacketID = new byte[4];
                        stream.Read(bPacketID, 0, 4);
                        var PacketID = BitConverter.ToInt32(bPacketID, 0);

                        switch (PacketID)
                        {
                            case Protocols.LOGIN_REQ:
                                var packet = Serializer.Deserialize<LoginReq>(stream);
                                var user = _handler.OnLoginReq(packet, ipPort);

                                if(!_authUsers.ContainsKey(ipPort))
                                    _authUsers.Add(ipPort, user);
                            break;

                            case Protocols.ZONECOUNT_REQ:
                                if (!CheckPrivileges(ipPort, GroupRole.Guest))
                                    goto default;

                                _handler.OnZoneCountReq(ipPort);

                                break;

                            default:
                                _watsonTcpServer.DisconnectClient(ipPort);
                                break;
                        }
                        Console.WriteLine("Received PID {0} from {1}", PacketID, ipPort);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return true;
        }

        public bool Send<T>(string ipPort, T packet)
        {
            int PacketID = ((Packet)(object)packet).PacketID;

            using (var stream = new MemoryStream())
            {
                var pid = BitConverter.GetBytes(PacketID);
                stream.Write(pid, 0, 4);
                Serializer.Serialize(stream, packet);

                _watsonTcpServer.Send(ipPort, stream.ToArray());
            }

            return true;
        }

        private bool CheckPrivileges(string ipPort, GroupRole min)
        {
            if(_authUsers.ContainsKey(ipPort) && _authUsers[ipPort] != null)
            {
                return _authUsers[ipPort].GroupRole >= min;
            }
            else
            {
                return false;
            }
        }
    }
}
