using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class UnbanIPAddressReq: Packet
    {
        public string IpAddress { get; set; }

        public UnbanIPAddressReq()
        {
            PacketId = Protocols.UNBANIPADDRESS_REQ;
        }
    }
}
