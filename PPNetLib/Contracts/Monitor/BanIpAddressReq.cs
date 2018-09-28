using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class BanIpAddressReq: Packet
    {
        [ProtoMember(1)]
        public string IpAddress { get; set; }

        public BanIpAddressReq()
        {
            PacketId = Protocols.BANIPADDRESS_REQ;
        }
    }
}
