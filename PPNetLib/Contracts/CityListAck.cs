using System;
using System.Collections.Generic;
using System.Text;
using PPNetLib.Prototypes;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class CityListAck: Packet
    {
        [ProtoMember(1, DynamicType = true)]
        public List<City> Cities { get; set; }

        public CityListAck()
        {
            PacketId = Protocols.CITYLIST_ACK;
        }
    }
}
