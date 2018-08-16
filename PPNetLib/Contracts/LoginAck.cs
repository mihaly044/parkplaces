using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class LoginAck: Packet
    {
        [ProtoMember(1)]
        public int Id;

        [ProtoMember(2)]
        public int GroupRole;

        [ProtoMember(3)]
        public int CreatorId;

        [ProtoMember(4)]
        public string UserName;

        [ProtoMember(5)]
        public DateTime LastLogin;

        [ProtoMember(6)]
        public bool IsAuthenticated;

        public LoginAck()
        {
            PacketID = Protocols.LOGIN_ACK;
        }
    }
}
