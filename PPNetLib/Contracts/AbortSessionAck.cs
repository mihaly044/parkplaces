using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class AbortSessionAck: Packet
    {
        public AbortSessionAck()
        {
            PacketId = Protocols.ABORTSESSION_ACK;
        }
    }
}
