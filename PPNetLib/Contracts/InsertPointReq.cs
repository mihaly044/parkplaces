using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class InsertPointReq : Packet
    {
        [ProtoMember(1)] public int ZoneId { get; set; }
        [ProtoMember(2)] public double Lat { get; set; }
        [ProtoMember(3)] public double Lng { get; set; }

        public InsertPointReq()
        {
            PacketId = Protocols.INSERTPOINT_REQ;
        }
    }
}
