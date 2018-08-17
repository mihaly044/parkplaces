using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class InsertZoneReq : Packet
    {
        [ProtoMember(1)]
        public string Zone;

        public InsertZoneReq()
        {
            PacketID = Protocols.INSERTZONE_REQ;
        }
    }
}
