﻿using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class LoginReq: Packet
    {
        [ProtoMember(1)]
        public string Username { get; set; }

        [ProtoMember(2)]
        public string Password { get; set; }

        [ProtoMember(3)]
        public bool Monitor { get; set; }

        public LoginReq()
        {
            PacketId = Protocols.LOGIN_REQ;
        }
    }
}
