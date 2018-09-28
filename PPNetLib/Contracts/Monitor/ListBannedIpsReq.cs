using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class ListBannedIpsReq: Packet
    {
        public ListBannedIpsReq()
        {
            PacketId = Protocols.LISTBANNEDIPS_REQ;
        }
    }
}
