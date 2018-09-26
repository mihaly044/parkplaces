using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class OnlineUsersReq: Packet
    {
        public OnlineUsersReq()
        {
            PacketId = Protocols.ONLINEUSERS_REQ;
        }
    }
}
