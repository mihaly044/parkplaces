﻿using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class UpdatePointReq: Packet
    {
        [ProtoMember(1)] public int PointId { get; set; }
        [ProtoMember(2)] public int Lat { get; set; }
        [ProtoMember(3)] public int Lng { get; set; }

        public UpdatePointReq()
        {
            PacketId = Protocols.UPDATEPOINT_REQ;
        }
    }
}
