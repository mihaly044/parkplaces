using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class DisconnectUserReq: Packet
    {
        [ProtoMember(1)]
        public string IpPort { get; set; }

        public DisconnectUserReq()
        {
            PacketId = Protocols.DISCONNECTUSER_REQ;
        }
    }
}
