using System;
using System.Collections.Generic;
using System.Text;
using PPNetLib.Prototypes;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class UpdateUserReq: Packet
    {
        [ProtoMember(1)]
        public User User { get; set; }

        [ProtoMember(2)]
        public bool ForceDisconnect { get; set; }

        public UpdateUserReq()
        {
            PacketId = Protocols.UPDATEUSER_REQ;
        }
    }
}
