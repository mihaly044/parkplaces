using System.Collections.Generic;
using System.Linq;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using PPServer.Database;
using Newtonsoft.Json;
using PPNetLib.Contracts.SynchroniseAcks;
using PPNetLib.Contracts.Monitor;
using PPNetLib;
using CommandLine;
using System;
using PPServer.CommandLine;

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

            if (user != null)
            {
                user.IpPort = ipPort;
                user.Monitor = packet.Monitor;
            }

            ack.User = user;
            _server.Send(ipPort, ack);
            return user;
        }

        public void OnZoneCountReq(string ipPort)
        {
            if(_server.MaxZones > 0)
            {
                _server.Send(ipPort, new ZoneCountAck() { ZoneCount = _server.MaxZones });
            }
            else
            {
                var count = Sql.Instance.GetZoneCount();
                _server.Send(ipPort, new ZoneCountAck() { ZoneCount = count });
            }
        }

        public void OnZoneListReq(string ipPort)
        {
            foreach(var zone in Server.Dto.Zones)
            {
                var zoneSerialized = JsonConvert.SerializeObject(zone, Converter.Settings);
                if(!_server.Send(ipPort, new ZoneListAck() { Zone = zoneSerialized }))
                {
                    ConsoleKit.Message(ConsoleKit.MessageType.ERROR, "Connection lost ...\n", zone.Id);
                    break;
                }
            }
        }

        public async void OnInsertZoneReqAsync(InsertZoneReq packet, User user)
        {
            var zone = JsonConvert.DeserializeObject<PolyZone> (packet.Zone, Converter.Settings);
            var pointIds = new List<int>();

            var id = await Sql.Instance.InsertZone(zone, delegate(int pointId)
            {
                pointIds.Add(pointId);
                return false;
            });

            zone.Id = id.ToString();

            lock(Server.Dto.Zones)
            {
                Server.Dto.Zones.Add(zone);
            }

            _server.Send(user, new InsertZoneAck()
            {
                ZoneId = id,
                PointIds = pointIds
            });

            _server.SendToEveryoneExcept(new ZoneInfoUpdatedAck()
            {
                ZoneId = id,
                Added = true,
                Data = JsonConvert.SerializeObject(zone, Converter.Settings)
            }, user.IpPort);
        }

        public void OnRemoveZoneReq(RemoveZoneReq packet, User user)
        {
            Sql.Instance.RemoveZone(packet.ZoneId);

            lock(Server.Dto.Zones)
            {
                var zone = Server.Dto.Zones.First(z => z.Id == packet.ZoneId.ToString());
                Server.Dto.Zones.Remove(zone);
            }

            _server.SendToEveryoneExcept(new ZoneInfoUpdatedAck()
            {
                ZoneId = packet.ZoneId,
                Removed = true,
            }, user.IpPort);
        }

        public void OnRemovePointReq(RemovePointReq packet, User user)
        {
            Sql.Instance.RemovePoint(packet.PointId, packet.Index, packet.ZoneId);

            lock(Server.Dto.Zones)
            {
                var zone = Server.Dto.Zones.First(z => z.Id == packet.ZoneId.ToString());
                zone.Geometry.Remove(zone.Geometry.First(g => g.Id == packet.PointId));
            }

            _server.SendToEveryoneExcept(new PointUpdatedAck()
            {
                Removed = true,
                ZoneId = packet.ZoneId,
                PointId = packet.PointId,
                Index = packet.Index
            }, user.IpPort);
        }

        public async void OnInsertPointReqAsync(InsertPointReq packet, User user)
        {
            var id =  await Sql.Instance.InsertPointAsync(packet.ZoneId, packet.Lat, packet.Lng, packet.Index);
            _server.Send(user, new InsertPointAck(){PointId = id});

            lock(Server.Dto.Zones)
            {
                var zone = Server.Dto.Zones.First(z => z.Id == packet.ZoneId.ToString());
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
            }, user.IpPort);
        }

        public void OnUpdatePointReq(UpdatePointReq packet, User user)
        {
            Sql.Instance.UpdatePoint(packet.PointId, packet.Lat, packet.Lng);

            lock(Server.Dto.Zones)
            {
                var zone = Server.Dto.Zones.First(z => z.Id == packet.ZoneId.ToString());
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
            }, user.IpPort);
        }

        public void OnUpdateZoneReq(UpdateZoneReq packet, User user)
        {
            var zone = JsonConvert.DeserializeObject<PolyZone>(packet.Zone, Converter.Settings);
            Sql.Instance.UpdateZoneInfo(zone);

            lock(Server.Dto.Zones)
            {
                var localDtoZone = Server.Dto.Zones.First(z => z.Zoneid == zone.Zoneid);
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
            }, user.IpPort);
        }

        public async void OnCityListReqAsync(User user)
        {
            var cities = await Sql.Instance.LoadCities();
            _server.Send(user, new CityListAck(){ Cities = cities });
        }

        public async void OnUserListReqAsync(User user)
        {
            var users = await Sql.Instance.LoadUsers();
            _server.Send(user, new UserListAck(){ Users = users });
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

        public void OnIsDuplicateUserReq(IsDuplicateUserReq packet, User u)
        {
            var user = packet.User;
            var isDuplicateUser = Sql.Instance.IsDuplicateUser(user);
            _server.Send(u, new IsDuplicateUserAck(){ IsDuplicateUser = isDuplicateUser });
        }

        public void OnOnlineUsersReq(User user)
        {
            _server.Send(user, new OnlineUsersAck(){ OnlineUsersList = _server.Guard.GetAuthUsers()});
        }

        public void OnDisconnectUserReq(DisconnectUserReq packet)
        {
            _server.DisconnectUser(packet.IpPort);
        }

        public void OnBanIPAddressReq(BanIpAddressReq packet, User u)
        {
            _server.Guard.BanIp(packet.IpAddress.Split(':')[0]);
            var user = _server.Guard.GetAuthUser(packet.IpAddress);
            if(user != null)
            {
                _server.DisconnectUser(packet.IpAddress);
            }

            var bannedIps = _server.Guard.GetBannedIps();
            _server.Send(u, new ListBannedIpsAck() { BannedIps = bannedIps });
        }

        public void OnListBannedIPsReq(User user)
        {
            var bannedIps = _server.Guard.GetBannedIps();
            _server.Send(user, new ListBannedIpsAck() { BannedIps = bannedIps });
        }

        public void OnUnbanIPAddressReq(UnbanIPAddressReq packet, User user)
        {
            _server.Guard.UnbanIp(packet.IpAddress.Split(':')[0]);

            var bannedIps = _server.Guard.GetBannedIps();
            _server.Send(user, new ListBannedIpsAck() { BannedIps = bannedIps });
        }

        public void OnCommandReq(CommandReq packet, User user)
        {
            var response = "";
            Parser.Default.ParseArguments<Echo, KickUser, BanIp, UnbanIp, Get, Shutdown>(packet.Command)
                .WithParsed<Echo>(o =>
                {
                    response = o.Text;
                    _server.Send(user, new CommandAck() { Response = response });
                })
                .WithParsed<KickUser>(o =>
                {
                    var kickUser = _server.Guard.GetAuthUserByName(o.UserName);
                    _server.DisconnectUser(kickUser.IpPort);
                })
                .WithParsed<BanIp>(o =>
                {
                    _server.Guard.BanIp(o.IPAddress);
                    _server.DisconnectUser(o.IPAddress);
                })
                .WithParsed<UnbanIp>(o =>
                {
                    _server.Guard.UnbanIp(o.IPAddress);
                })
                .WithParsed<Get>(o =>
                {
                    var get = "";
                    if (o.OnlineUsers)
                    {
                        var users = _server.Guard.GetAuthUsers();
                        get = $"Online: {users.Count}[{string.Join(", ", users)}]";
                    } else if (o.ZoneCount)
                    {
                        get = "Zone count: " + Server.Dto.Zones.Count.ToString();
                    }
                    else if (o.MemUsage)
                    {
                        get = $"Total memory usage is {GC.GetTotalMemory(true) / 1024 / 1024} MB";
                    }
                    else
                    {
                        get = "Unknown value requested";
                    }
                    _server.Send(user, new CommandAck() { Response = get });
                })
                .WithParsed<Shutdown>(o =>
                {
                    if (o.Seconds > 0)
                        _server.AnnounceShutdownAck(o.Seconds);
                    else
                        _server.Shutdown();
                   
                })
                .WithNotParsed ((errs) => {
                    _server.Send(user, new CommandAck() { Response = errs.ToString() });
                });
        }
    }
}
