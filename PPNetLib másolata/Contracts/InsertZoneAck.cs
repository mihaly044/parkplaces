using System.Collections.Generic;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class InsertZoneAck : Packet
    {
        [ProtoMember(1)]
        public int ZoneId { get; set; }

        [ProtoMember(2)]
        public List<int> PointIds { get; set; }

        public InsertZoneAck()
        {
            PacketId = Protocols.INSERTZONE_ACK;
        }
    }
}
