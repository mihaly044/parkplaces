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
            PacketId = Protocols.ZONELIST_REQ;
        }
    }
}
