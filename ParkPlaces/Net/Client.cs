using PPNetLib;
using ProtoBuf;
using System;
using System.Configuration;
using System.IO;
using watsontcp_dotnetcore.Tcp;
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
        private bool _offlineMode;
        private bool _forceDisconnect;

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

            SetOfflineMode(false);
        }

        public void SetOfflineMode(bool offlineMode)
        {
            _offlineMode = offlineMode;
        }

        public bool IsConnected()
        {
            return _watsonTcpClient != null && _watsonTcpClient.IsConnected();
        }

        public void Disconnect()
        {
            if(IsConnected())
            {
                _forceDisconnect = true;
                _watsonTcpClient.Dispose();
            }
        }

        public void ResetLoginAcks()
        {
            OnLoginAck = null;
            OnLoginDuplicateAck = null;
        }

        public bool GetOfflineMode()
        {
            return _offlineMode;
        }

        private bool MessageReceived(byte[] data)
        {
            if (data == null || data.Length <= 0) return false;

            try
            {
                using (var stream = new MemoryStream(data))
                {
                    var bProtocolVersion = new byte[4];
                    stream.Read(bProtocolVersion, 0, 4);
                    var protocolVersion = BitConverter.ToInt32(bProtocolVersion, 0);
                    if (protocolVersion != Protocol.Version)
                        throw new Exception("Invalid protocol version");

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
                        case Protocols.INSERTPOINT_ACK:
                            var pointInsertAck = Serializer.Deserialize<PPNetLib.Contracts.InsertPointAck>(stream);
                            OnPointInsertAck?.Invoke(pointInsertAck);
                            OnPointInsertAck = null;
                            break;
                        case Protocols.CITYLIST_ACK:
                            var cityListAck = Serializer.Deserialize<PPNetLib.Contracts.CityListAck>(stream);
                            OnCityListAck?.Invoke(cityListAck);
                            break;
                        case Protocols.USERLIST_ACK:
                            var userListAck = Serializer.Deserialize<PPNetLib.Contracts.UserListAck>(stream);
                            OnUserListAck?.Invoke(userListAck);
                            break;
                         case Protocols.ISDUPLICATEUSER_ACK:
                            var isDuplicateUserAck = Serializer.Deserialize<PPNetLib.Contracts.IsDuplicateUserAck>(stream);
                            OnIsDuplicateUserAck?.Invoke(isDuplicateUserAck);
                            OnIsDuplicateUserAck = null;
                            break;
                        case Protocols.POINTUPDATED_ACK:
                            var pointUpdatedAck =
                                Serializer.Deserialize<PPNetLib.Contracts.SynchroniseAcks.PointUpdatedAck>(stream);
                            OnPointUpdatedAck?.Invoke(pointUpdatedAck);
                            break;
                        case Protocols.ZONEINFOUPDATED_ACK:
                            var zoneInfoUpdatedAck =
                                Serializer.Deserialize<PPNetLib.Contracts.SynchroniseAcks.ZoneInfoUpdatedAck>(stream);
                            OnZoneInfoUpdatedAck?.Invoke(zoneInfoUpdatedAck);
                            break;
                        case Protocols.LOGINDUPLICATE_ACK:
                            OnLoginDuplicateAck?.Invoke();
                            break;
                        case Protocols.SHUTDOWN_ACK:
                            var shutdownAck = Serializer.Deserialize<PPNetLib.Contracts.ShutdownAck>(stream);
                            OnShutdownAck?.Invoke(shutdownAck);
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
            if(!_forceDisconnect)
                OnConnectionError?.Invoke(new Exception());
            return false;
        }

        private bool ServerConnected()
        {
            return false;
        }

        public bool Send<T>(T packet)
        {
            if (_offlineMode)
                return false;

            var packetId = (int)((Packet)(object)packet).PacketId;

            using (var stream = new MemoryStream())
            {
                var protocolVersion = BitConverter.GetBytes(Protocol.Version);
                stream.Write(protocolVersion, 0, 4);

                var pid = BitConverter.GetBytes(packetId);
                stream.Write(pid, 0, 4);
                Serializer.Serialize(stream, packet);

                _watsonTcpClient.Send(stream.ToArray());
            }

            return true;
        }
    }
}
