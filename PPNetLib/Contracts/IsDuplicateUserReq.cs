using System;
using System.Collections.Generic;
using System.Text;
using PPNetLib.Prototypes;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class IsDuplicateUserReq: Packet
    {
        [ProtoMember(1)]
        public User User;

        public IsDuplicateUserReq()
        {
            PacketId = Protocols.ISDUPLICATEUSER_REQ;
        }
    }
}
