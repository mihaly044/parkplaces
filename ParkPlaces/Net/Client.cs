using PPNetLib;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatsonTcp;
using PPNetLib.Extensions;
using PPNetLib.Contracts;

namespace ParkPlaces.Net
{
    public class Client
    {
        private WatsonTcpClient _watsonTcpClient;
        private string _serverIp;
        private const int _serverPort = 11000;

        public Client()
        {
            _serverIp = ConfigurationManager.AppSettings["ServerIP"];
            _watsonTcpClient = new WatsonTcpClient(_serverIp, _serverPort, ServerConnected, ServerDisconnected, MessageReceived, true);
        }

        private bool MessageReceived(byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                try
                {
                    var PacketID = BitConverter.ToInt32(data, 0);

                    var stream = new MemoryStream();
                    stream.Write(data, 0, data.Length);

                    switch (PacketID)
                    {
                        case Protocols.LOGIN_REQ:
                            var packet = Serializer.Deserialize<LoginAck>(stream);
                            //OnLogin(packet);

                            break;
                    }

                    Console.WriteLine("Received PID {0} from {1}", PacketID);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error...");
                }
            }

            return true;
        }

        private bool ServerDisconnected()
        {
            return false;
        }

        private bool ServerConnected()
        {
            return false;
        }

        public bool Send<T>(string ipPort, T packet)
        {
            int PacketID = ((Packet)(object)packet).PacketID;

            var stream = new MemoryStream();
            var pid = BitConverter.GetBytes(PacketID);
            stream.Write(pid, 0, 4);
            Serializer.Serialize(stream, packet);

            _watsonTcpClient.Send(stream.ToByteArray());

            return true;
        }
    }
}
