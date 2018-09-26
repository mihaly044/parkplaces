using System.Collections.Generic;
using PPNetLib.Prototypes;
using ProtoBuf;

namespace PPNetLib.Contracts.SynchroniseAcks
{
    [ProtoContract]
    public class OnlineUsersAck: Packet
    {
        [ProtoMember(1)]
        public List<User> OnlineUsersList;

        public OnlineUsersAck()
        {
            PacketId = Protocols.ONLINEUSERS_ACK;
        }
    }
}
