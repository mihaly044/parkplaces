using ProtoBuf;

namespace PPNetLib.Contracts.SynchroniseAcks
{
    [ProtoContract]
    public class ZoneInfoUpdatedAck : Packet
    {
        [ProtoMember(1)]
        public int ZoneId { get; set; }

        [ProtoMember(2)]
        public string Data { get; set; }

        [ProtoMember(3)]
        public bool Removed { get; set; }

        [ProtoMember(4)]
        public bool Added { get; set; }

        public ZoneInfoUpdatedAck()
        {
            PacketId = Protocols.ZONEINFOUPDATED_ACK;
        }
    }
}
