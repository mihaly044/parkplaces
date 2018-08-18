using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class InsertZoneAck : Packet
    {
        [ProtoMember(1)]
        public int ZoneId { get; set; }

        public InsertZoneAck()
        {
            PacketId = Protocols.INSERTZONE_ACK;
        }
    }
}
