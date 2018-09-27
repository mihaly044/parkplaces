using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
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
