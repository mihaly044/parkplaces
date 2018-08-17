using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class ZoneListReq : Packet
    {
        public ZoneListReq()
        {
            PacketID = Protocols.ZONELIST_REQ;
        }
    }
}
