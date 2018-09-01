using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class InsertZoneReq : Packet
    {
        [ProtoMember(1)]
        public string Zone;

        public InsertZoneReq()
        {
            PacketId = Protocols.INSERTZONE_REQ;
        }
    }
}
