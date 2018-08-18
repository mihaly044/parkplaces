using System;
using System.Collections.Generic;
using System.Text;
using PPNetLib.Prototypes;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class UserListAck: Packet
    {
        [ProtoMember(1)]
        public List<User> Users { get; set; }

        public UserListAck()
        {
            PacketId = Protocols.USERLIST_ACK;
        }
    }
}
