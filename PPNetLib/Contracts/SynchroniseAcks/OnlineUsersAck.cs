using System.Collections.Generic;
using PPNetLib.Prototypes;
using ProtoBuf;

namespace PPNetLib.Contracts.SynchroniseAcks
{
    [ProtoContract]
    public class OnlineUsersAck: Packet
    {
        public List<User> OnlineUsersList;

        public OnlineUsersAck()
        {
            PacketId = Protocols.ONLINEUSERS_ACK;
        }
    }
}
