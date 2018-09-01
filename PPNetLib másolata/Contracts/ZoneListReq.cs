using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class ZoneListReq : Packet
    {
        public ZoneListReq()
        {
            PacketId = Protocols.ZONELIST_REQ;
        }
    }
}
