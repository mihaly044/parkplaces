﻿using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class RemovePointReq: Packet
    {
        [ProtoMember(1)]
        public int PointId { get; set; }

        [ProtoMember(2)]
        public int ZoneId { get; set; }

        [ProtoMember(4)]
        public int Index { get; set; }

        public RemovePointReq()
        {
            PacketId = Protocols.REMOVEPOINT_REQ;
        }
    }
}
