using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using PPServer.LocalPrototypes;
using PPServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PPServer
{
    public class Handler
    {
        private Server _server;

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
    }
}
