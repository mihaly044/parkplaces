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
    public partial class Client
    {
        private static Client _instance;
        public static Client Instance => _instance ?? (_instance = new Client());

        private WatsonTcpClient _watsonTcpClient;
        private readonly string _serverIp;
        private readonly int _serverPort;

        public Client()
        {
            _serverIp = ConfigurationManager.AppSettings["ServerIP"];
            _serverPort = int.TryParse(ConfigurationManager.AppSettings["ServerPort"], out var port) ? port : 11000;
        }

        public void Connect(bool throwsException=false)
        {
            try
            {
                _watsonTcpClient = new WatsonTcpClient(_serverIp, _serverPort, ServerConnected, ServerDisconnected, MessageReceived, true);
            }
            catch (Exception e)
            {
                OnConnectionError?.Invoke(e);
                if (throwsException)
                    throw;
            }
        }

        private bool MessageReceived(byte[] data)
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
                        case Protocols.LOGIN_ACK:
                            var loginAck = Serializer.Deserialize<PPNetLib.Contracts.LoginAck>(stream);
                            OnLoginAck?.Invoke(loginAck);
                            break;
                        case Protocols.ZONECOUNT_ACK:
                            var zoneCountAck = Serializer.Deserialize<PPNetLib.Contracts.ZoneCountAck>(stream);
                            OnZoneCountAck?.Invoke(zoneCountAck);
                            break;
                        case Protocols.ZONELIST_ACK:
                            var zoneListAck = Serializer.Deserialize<PPNetLib.Contracts.ZoneListAck>(stream);
                            OnZoneListAck?.Invoke(zoneListAck);
                            break;
                        case Protocols.INSERTZONE_ACK:
                            var zoneInsertAck = Serializer.Deserialize<PPNetLib.Contracts.InsertZoneAck>(stream);
                            OnZoneInsertAck?.Invoke(zoneInsertAck);
                            OnZoneInsertAck = null;
                            break;
                    }
                    Debug.WriteLine("Received PID {0}", packetId);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return true;
        }

        private bool ServerDisconnected()
        {
            OnConnectionError?.Invoke(new Exception());
            return false;
        }

        private bool ServerConnected()
        {
            return false;
        }

        public bool Send<T>(T packet)
        {
            var packetId = (int)((Packet)(object)packet).PacketId;

            using (var stream = new MemoryStream())
            {
                var pid = BitConverter.GetBytes(packetId);
                stream.Write(pid, 0, 4);
                Serializer.Serialize(stream, packet);

                _watsonTcpClient.Send(stream.ToArray());
            }

            return true;
        }
    }
}
