using System;

#pragma warning disable 67

namespace ParkPlaces.Net
{
    public partial class Client
    {
        public delegate void ConnectionError(Exception e);
        public event ConnectionError OnConnectionError;

        public delegate void LoginAck(PPNetLib.Contracts.LoginAck ack);
        public event LoginAck OnLoginAck;

        public delegate void ZoneCountAck(PPNetLib.Contracts.ZoneCountAck ack);
        public event ZoneCountAck OnZoneCountAck;

        public delegate void ZoneListAck(PPNetLib.Contracts.ZoneListAck ack);
        public event ZoneListAck OnZoneListAck;

        public delegate void ZoneInsertAck(PPNetLib.Contracts.InsertZoneAck ack);
        public event ZoneInsertAck OnZoneInsertAck;

        public delegate void CityListAck(PPNetLib.Contracts.CityListAck ack);
        public event CityListAck OnCityListAck;

        public delegate void InsertPointAck(PPNetLib.Contracts.InsertPointAck ack);
        public event InsertPointAck OnPointInsertAck;

        public delegate void UserListAck(PPNetLib.Contracts.UserListAck ack);
        public event UserListAck OnUserListAck;

        public delegate void IsDuplicateUserAck(PPNetLib.Contracts.IsDuplicateUserAck ack);
        public event IsDuplicateUserAck OnIsDuplicateUserAck;

        public delegate void PointUpdatedAck(PPNetLib.Contracts.SynchroniseAcks.PointUpdatedAck ack);
        public event PointUpdatedAck OnPointUpdatedAck;

        public delegate void ZoneInfoUpdatedAAck(PPNetLib.Contracts.SynchroniseAcks.ZoneInfoUpdatedAck ack);
        public event ZoneInfoUpdatedAAck OnZoneInfoUpdatedAck;

        public delegate void LoginDuplicateAck();
        public event LoginDuplicateAck OnLoginDuplicateAck;

        public delegate void ShutdownAck(PPNetLib.Contracts.ShutdownAck ack);
        public event ShutdownAck OnShutdownAck;

        public delegate void ServerMonitorAck(PPNetLib.Contracts.ServerMonitorAck ack);
        public event ServerMonitorAck OnServerMonitorAck;
    }
}
