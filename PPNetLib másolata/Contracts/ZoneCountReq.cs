using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class ZoneCountReq: Packet
    {
        public ZoneCountReq()
        {
            PacketId = Protocols.ZONECOUNT_REQ;
        }
    }
}
