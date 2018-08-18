using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using PPNetLib.Contracts;

namespace PPNetLib
{
    [ProtoContract]
    public class Packet
    {
        [ProtoMember(1)]
        public Protocols PacketId { get; set; }

        [ProtoMember(2)]
        public int ProtocolVersion { get; set; }

        public Packet()
        {
            ProtocolVersion = Protocol.Version;
        }
    }
}
