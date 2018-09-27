using System.Collections.Generic;
using System.Linq;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using PPServer.Database;
using Newtonsoft.Json;
using PPNetLib.Contracts.SynchroniseAcks;
using PPNetLib.Contracts.Monitor;
using PPNetLib;

namespace PPServer
{
    public class Handler
    {
        private readonly Server _server;

        public Handler(Server s)
        {
            _server = s;
        }

        public User OnLoginReq(LoginReq packet, string ipPort, Dictionary<string, User> users)
        {
            if(users.FirstOrDefault(u => u.Value.UserName == packet.Username && u.Value.Monitor != packet.Monitor).Value != null)
            {
                _server.Send(ipPort, new LoginDuplicateAck());
            }
            else
            {
                var ack = new LoginAck();
                var user = Sql.Instance.AuthenticateUser(packet.Username, packet.Password);
                
                if(user != null)
                {
                    user.Monitor = packet.Monitor;
                    user.IpPort = ipPort;
                }

                ack.User = user;
                _server.Send(ipPort, ack);
                return user;
            }
            return null;
        }

        public void OnZoneCountReq(string ipPort)
        {
            var count = Sql.Instance.GetZoneCount();
            _server.Send(ipPort, new ZoneCountAck() { ZoneCount = count });
        }

        public void OnZoneListReq(string ipPort)
        {
            foreach(var zone in _server.Dto.Zones)
            {
                // TODO: Find out why sending fails when a client reconnects
                var zoneSerialized = JsonConvert.SerializeObject(zone, Converter.Settings);
                //ConsoleKit.Message(ConsoleKit.MessageType.DEBUG, "Now sending zone id {0}\n", zone.Id);
                //Thread.Sleep(1);
                if(!_server.Send(ipPort, new ZoneListAck() { Zone = zoneSerialized }))
                {
                    ConsoleKit.Message(ConsoleKit.MessageType.ERROR, "Connection lost ...\n", zone.Id);
                    break;
                }
            }
        }

        public async void OnInsertZoneReqAsync(InsertZoneReq packet, string ipPort)
        {
            var zone = JsonConvert.DeserializeObject<PolyZone> (packet.Zone, Converter.Settings);
            var pointIds = new List<int>();

            var id = await Sql.Instance.InsertZone(zone, delegate(int pointId)
            {
                pointIds.Add(pointId);
                return false;
            });

            zone.Id = id.ToString();

            lock(_server.Dto.Zones)
            {
                _server.Dto.Zones.Add(zone);
            }

            _server.Send(ipPort, new InsertZoneAck()
            {
                ZoneId = id,
                PointIds = pointIds
            });

            _server.SendToEveryoneExcept(new ZoneInfoUpdatedAck()
            {
                ZoneId = id,
                Added = true,
                Data = JsonConvert.SerializeObject(zone, Converter.Settings)
            }, ipPort);
        }

        public void OnRemoveZoneReq(RemoveZoneReq packet, string ipPort)
        {
            Sql.Instance.RemoveZone(packet.ZoneId);

            lock(_server.Dto.Zones)
            {
                var zone = _server.Dto.Zones.First(z => z.Id == packet.ZoneId.ToString());
                _server.Dto.Zones.Remove(zone);
            }

            _server.SendToEveryoneExcept(new ZoneInfoUpdatedAck()
            {
                ZoneId = packet.ZoneId,
                Removed = true,
            }, ipPort);
        }

        public void OnRemovePointReq(RemovePointReq packet, string ipPort)
        {
            Sql.Instance.RemovePoint(packet.PointId, packet.Index, packet.ZoneId);

            lock(_server.Dto.Zones)
            {
                var zone = _server.Dto.Zones.First(z => z.Id == packet.ZoneId.ToString());
                zone.Geometry.Remove(zone.Geometry.First(g => g.Id == packet.PointId));
            }

            _server.SendToEveryoneExcept(new PointUpdatedAck()
            {
                Removed = true,
                ZoneId = packet.ZoneId,
                PointId = packet.PointId,
                Index = packet.Index
            }, ipPort);
        }

        public async void OnInsertPointReqAsync(InsertPointReq packet, string ipPort)
        {
            var id =  await Sql.Instance.InsertPointAsync(packet.ZoneId, packet.Lat, packet.Lng, packet.Index);
            _server.Send(ipPort, new InsertPointAck(){PointId = id});

            lock(_server.Dto.Zones)
            {
                var zone = _server.Dto.Zones.First(z => z.Id == packet.ZoneId.ToString());
                zone.Geometry.Insert(packet.Index, new Geometry(packet.Lat, packet.Lng, id));
            }

            _server.SendToEveryoneExcept(new PointUpdatedAck()
            {
                Added = true,
                PointId = id,
                Lat = packet.Lat,
                Lng = packet.Lng,
                ZoneId = packet.ZoneId,
                Index = packet.Index
            }, ipPort);
        }

        public void OnUpdatePointReq(UpdatePointReq packet, string ipPort)
        {
            Sql.Instance.UpdatePoint(packet.PointId, packet.Lat, packet.Lng);

            lock(_server.Dto.Zones)
            {
                var zone = _server.Dto.Zones.First(z => z.Id == packet.ZoneId.ToString());
                var geometry = zone.Geometry.First(g => g.Id == packet.PointId);
                geometry.Lat = packet.Lat;
                geometry.Lng = packet.Lng;
            }

            _server.SendToEveryoneExcept(new PointUpdatedAck()
            {
                PointId = packet.PointId,
                Lat = packet.Lat,
                Lng = packet.Lng,
                ZoneId = packet.ZoneId
            }, ipPort);
        }

        public void OnUpdateZoneReq(UpdateZoneReq packet, string ipPort)
        {
            var zone = JsonConvert.DeserializeObject<PolyZone>(packet.Zone, Converter.Settings);
            Sql.Instance.UpdateZoneInfo(zone);

            lock(_server.Dto.Zones)
            {
                var localDtoZone = _server.Dto.Zones.First(z => z.Zoneid == zone.Zoneid);
                localDtoZone.Color = zone.Color;
                localDtoZone.Description = zone.Description;
                localDtoZone.Distance = zone.Distance;
                localDtoZone.Fee = zone.Fee;
                localDtoZone.ServiceNa = zone.ServiceNa;
                localDtoZone.Telepules = zone.Telepules;
                localDtoZone.Timetable = zone.Timetable;
            }

            _server.SendToEveryoneExcept(new ZoneInfoUpdatedAck()
            {
                ZoneId = int.Parse(zone.Id),
                Data = JsonConvert.SerializeObject(zone, Converter.Settings)
            }, ipPort);
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

        public void OnOnlineUsersReq(string ipPort)
        {
            _server.Send(ipPort, new OnlineUsersAck(){ OnlineUsersList = _server.GetAuthUsers()});
        }
    }
}
