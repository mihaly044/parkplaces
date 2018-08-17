using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class ZoneCountReq: Packet
    {
        public ZoneCountReq()
        {
            PacketID = Protocols.ZONECOUNT_REQ;
        }
    }
}
