using System.Collections.Generic;
using PPNetLib.Prototypes;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class CityListAck: Packet
    {
        [ProtoMember(1)]
        public List<City> Cities { get; set; }

        public CityListAck()
        {
            PacketId = Protocols.CITYLIST_ACK;
        }
    }
}
