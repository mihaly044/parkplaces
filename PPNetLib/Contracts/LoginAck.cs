using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class LoginAck: Packet
    {
        public enum Status
        {
            BadPassword,
            Unauthorized,
            Success
        }

        [ProtoMember(1)]
        public Status LoginStatus;

        public LoginAck(): base(Protocols.LOGIN_ACK)
        {

        }
    }
}
