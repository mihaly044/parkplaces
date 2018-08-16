﻿using System;
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
using PPNetLib.Extensions;

namespace PPServer
{
    public class Server
    {
        private string _ip;
        private int _port;
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
                    var PacketID = BitConverter.ToInt32(data, 0);

                    var stream = new MemoryStream();
                    stream.Write(data, 0, data.Length);

                    switch (PacketID)
                    {
                        case Protocols.LOGIN_REQ:
                            var packet = Serializer.Deserialize<LoginReq>(stream);
                            //OnLogin(packet);

                            break;
                    }

                    Console.WriteLine("Received PID {0} from {1}", PacketID, ipPort);
                }
                catch(Exception)
                {
                    Console.WriteLine("Error...");
                }
            }

            return true;
        }

        public bool Send<T>(string ipPort, T packet)
        {
            int PacketID = ((Packet)(object)packet).PacketID;

            var stream = new MemoryStream();
            var pid = BitConverter.GetBytes(PacketID);
            stream.Write(pid, 0, 4);
            Serializer.Serialize(stream, packet);

            _watsonTcpServer.Send(ipPort, stream.ToByteArray());

            return true;
        }
    }
}
