﻿using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using PPServer.LocalPrototypes;
using PPServer.Database;
using Newtonsoft.Json;

namespace PPServer
{
    public class Handler
    {
        private readonly Server _server;

        public Handler(Server s)
        {
            _server = s;
        }

        public User OnLoginReq(LoginReq packet, string ipPort)
        {
            var ack = new LoginAck();
            var user = Sql.Instance.AuthenticateUser(packet.Username, packet.Password);
            ack.User = user;

            _server.Send(ipPort, ack);

            return user;
        }

        public void OnZoneCountReq(string ipPort)
        {
            var count = Sql.Instance.GetZoneCount();
            _server.Send(ipPort, new ZoneCountAck() { ZoneCount = count });
        }

        public void OnZoneListReq(string ipPort)
        {
            Sql.Instance.LoadZones( (PolyZone zone) => {
                var zoneSerialized = JsonConvert.SerializeObject(zone, Converter.Settings);
                _server.Send(ipPort, new ZoneListAck() { Zone = zoneSerialized });
                return true;
            } );
        }

        public async void OnInsertZoneReqAsync(InsertZoneReq packet, string ipPort)
        {
            var zone = JsonConvert.DeserializeObject<PolyZone> (packet.Zone, Converter.Settings);
            var id = await Sql.Instance.InsertZone(zone);

            _server.Send(ipPort, new InsertZoneAck() { ZoneId = id });
        }

        public void OnRemoveZoneReq(RemoveZoneReq packet)
        {
            Sql.Instance.RemoveZone(packet.ZoneId);
        }

        public void OnRemovePointReq(RemovePointReq packet)
        {
            Sql.Instance.RemovePoint(packet.PointId);
        }

        public async void OnInsertPointReqAsync(InsertPointReq packet, string ipPort)
        {
            var id =  await Sql.Instance.InsertPointAsync(packet.ZoneId, packet.Lat, packet.Lng);
            _server.Send(ipPort, new InsertPointAck(){PointId = id});
        }

        public void OnUpdatePointReq(UpdatePointReq packet)
        {
            Sql.Instance.UpdatePoint(packet.PointId, packet.Lat, packet.Lng);
        }

        public void OnUpdateZoneReq(UpdateZoneReq packet)
        {
            var zone = JsonConvert.DeserializeObject<PolyZone>(packet.Zone, Converter.Settings);
            Sql.Instance.UpdateZoneInfo(zone);
        }

        public async void OnCityListReqAsync(string ipPort)
        {
            var cities = await Sql.Instance.LoadCities();
            _server.Send(ipPort, new CityListAck(){ Cities = cities });
        }

        public async void OnUserListReqAsync(string ipPort)
        {
            var users = await Sql.Instance.LoadUsers();
            _server.Send(ipPort, new UserListAck(){ Users = users });
        }

        public void OnInsertUserReq(InsertUserReq packet)
        {
            var user = packet.User;
            var creatorId = packet.CreatorId;
            Sql.Instance.InsertUser(user, creatorId);
        }

        public void OnRemoveUserReq(RemoveUserReq packet)
        {
            var id = packet.UserId;
            Sql.Instance.RemoveUser(id);
        }

        public void OnUpdateUserReq(UpdateUserReq packet)
        {
            var user = packet.User;
            Sql.Instance.UpdateUser(user);

            if(packet.ForceDisconnect)
                _server.DisconnectUser(user.Id);
        }

        public void IsDuplicateUserReq(IsDuplicateUserReq packet, string ipPort)
        {
            var user = packet.User;
            var isDuplicateUser = Sql.Instance.IsDuplicateUser(user);
            _server.Send(ipPort, new IsDuplicateUserAck(){ IsDuplicateUser = isDuplicateUser });
        }
    }
}
