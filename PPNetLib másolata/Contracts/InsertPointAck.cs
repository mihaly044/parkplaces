using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class InsertPointAck : Packet
    {
        [ProtoMember(1)]
        public int PointId { get; set; }

        public InsertPointAck()
        {
            PacketId = Protocols.INSERTPOINT_ACK;
        }
    }
}
