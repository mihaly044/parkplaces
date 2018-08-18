using PPNetLib.Prototypes;
using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class LoginAck: Packet
    {
        [ProtoMember(1)]
        public User User;

        public LoginAck()
        {
            PacketId = Protocols.LOGIN_ACK;
        }
    }
}
