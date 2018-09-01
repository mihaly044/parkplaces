using ProtoBuf;

namespace PPNetLib
{
    [ProtoContract]
    public class Packet
    {
        [ProtoMember(1)]
        public Protocols PacketId { get; protected set; }

        protected Packet()
        {
        }
    }
}
