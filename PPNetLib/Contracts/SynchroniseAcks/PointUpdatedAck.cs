using ProtoBuf;

namespace PPNetLib.Contracts.SynchroniseAcks
{
    [ProtoContract]
    public class PointUpdatedAck: Packet
    {
        [ProtoMember(1)]
        public int ZoneId { get; set; }

        [ProtoMember(2)]
        public int PointId { get; set; }

        [ProtoMember(3)]
        public double Lat { get; set; }

        [ProtoMember(4)]
        public double Lng { get; set; }

        [ProtoMember(5)]
        public bool Removed { get; set; }

        [ProtoMember(6)]
        public bool Added { get; set; }

        public PointUpdatedAck()
        {
            PacketId = Protocols.POINTUPDATED_ACK;
        }
    }
}
