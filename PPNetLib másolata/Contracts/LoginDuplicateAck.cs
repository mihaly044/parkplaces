using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class LoginDuplicateAck: Packet
    {
        public LoginDuplicateAck()
        {
            PacketId = Protocols.LOGINDUPLICATE_ACK;
        }
    }
}
