using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class ShutdownAck: Packet
    {
        [ProtoMember(1)]
        public int Seconds { get; set; }

        public ShutdownAck()
        {
            PacketId = Protocols.SHUTDOWN_ACK;
        }
    }
}
