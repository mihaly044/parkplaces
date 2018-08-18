using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class RemovePointReq: Packet
    {
        [ProtoMember(1)]
        public int PointId { get; set; }

        public RemovePointReq()
        {
            PacketId = Protocols.REMOVEPOINT_REQ;
        }
    }
}
