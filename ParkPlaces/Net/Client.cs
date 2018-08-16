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
using PPNetLib.Contracts;
using System.Diagnostics;

namespace ParkPlaces.Net
{
    public class Client
    {
        private WatsonTcpClient _watsonTcpClient;
        private string _serverIp;
        private const int _serverPort = 11000;

        public delegate void Test();
        public event Test OnTest;

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
                    using (var stream = new MemoryStream(data))
                    {
                        var bPacketID = new byte[4];
                        stream.Read(bPacketID, 0, 4);
                        var PacketID = BitConverter.ToInt32(bPacketID, 0);

                        switch (PacketID)
                        {
                            case Protocols.LOGIN_ACK:
                                var packet = Serializer.Deserialize<LoginAck>(stream);
                                OnTest.Invoke();

                                break;
                        }
                        Debug.WriteLine("Received PID {0} from {1}", PacketID);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
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

        public bool Send<T>(T packet)
        {
            int PacketID = ((Packet)(object)packet).PacketID;

            var stream = new MemoryStream();
            var pid = BitConverter.GetBytes(PacketID);
            stream.Write(pid, 0, 4);
            Serializer.Serialize(stream, packet);

            _watsonTcpClient.Send(stream.ToArray());

            return true;
        }
    }
}
