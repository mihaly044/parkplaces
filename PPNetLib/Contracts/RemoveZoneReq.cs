using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class RemoveZoneReq : Packet
    {
        [ProtoMember(1)]
        public int ZoneId { get; set; }

        public RemoveZoneReq()
        {
            PacketId = Protocols.REMOVEZONE_REQ;
        }
    }
}
