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

        public Packet()
        {
        }
    }
}
