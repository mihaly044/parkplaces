using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
