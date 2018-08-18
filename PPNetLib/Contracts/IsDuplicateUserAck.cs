using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class IsDuplicateUserAck: Packet
    {
        [ProtoMember(1)]
        public bool IsDuplicateUser { get; set; }

        public IsDuplicateUserAck()
        {
            PacketId = Protocols.ISDUPLICATEUSER_ACK;
        }
    }
}
