using ProtoBuf;
using System.Collections.Generic;

namespace PPNetLib.Contracts
{
    [ProtoContract]
    public class ZoneListAck : Packet
    {
        [ProtoMember(1)]
        public List<string> Zone { get; set; } // JSON serialized zone

        public ZoneListAck()
        {
            PacketId = Protocols.ZONELIST_ACK;
        }
    }
}
