using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using PPServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
