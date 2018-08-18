using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class LoginAck: Packet
    {
        [ProtoMember(1, DynamicType = true)]
        public object User;

        public LoginAck()
        {
            PacketId = Protocols.LOGIN_ACK;
        }
    }
}
