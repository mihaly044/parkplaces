using ProtoBuf;

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
