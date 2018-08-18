using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class ZoneCountAck : Packet
    {
        [ProtoMember(1)]
        public int ZoneCount { get; set; }

        public ZoneCountAck()
        {
            PacketId = Protocols.ZONECOUNT_ACK;
        }
    }
}
