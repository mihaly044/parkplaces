using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class RemoveUserReq: Packet
    {
        [ProtoMember(1)]
        public int UserId { get; set; }

        public RemoveUserReq()
        {
            PacketId = Protocols.REMOVEUSER_REQ;
        }
    }
}
