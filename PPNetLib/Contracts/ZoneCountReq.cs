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
            PacketId = Protocols.ZONECOUNT_REQ;
        }
    }
}
