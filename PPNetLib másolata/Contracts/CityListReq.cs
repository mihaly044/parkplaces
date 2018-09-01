using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class CityListReq: Packet
    {
        public CityListReq()
        {
            PacketId = Protocols.CITYLIST_REQ;
        }
    }
}
