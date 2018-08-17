using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class InsertZoneAck : Packet
    {
        [ProtoMember(1)]
        public int ZoneId { get; set; }

        public InsertZoneAck()
        {
            PacketID = Protocols.INSERTZONE_ACK;
        }
    }
}
