using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class UpdatePointReq: Packet
    {
        [ProtoMember(1)] public int PointId { get; set; }
        [ProtoMember(2)] public double Lat { get; set; }
        [ProtoMember(3)] public double Lng { get; set; }

        public UpdatePointReq()
        {
            PacketId = Protocols.UPDATEPOINT_REQ;
        }
    }
}
