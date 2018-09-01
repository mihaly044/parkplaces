using ProtoBuf;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class ZoneListAck : Packet
    {
        [ProtoMember(1)]
        public string Zone { get; set; } // JSON serialized zone

        public ZoneListAck()
        {
            PacketId = Protocols.ZONELIST_ACK;
        }
    }
}
