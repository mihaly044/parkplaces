using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class UpdateZoneReq: Packet
    {
        [ProtoMember(1)] public string Zone { get; set; }

        public UpdateZoneReq()
        {
            PacketId = Protocols.UPDATEZONE_REQ;
        }
    }
}
