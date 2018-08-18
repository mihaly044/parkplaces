using System;
using System.Collections.Generic;
using System.Text;
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
