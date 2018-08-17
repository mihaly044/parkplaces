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
                                _handler.OnLoginReq(packet, ipPort);
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
    }
}
