using PPNetLib.Prototypes;
using ProtoBuf;
using System.Collections.Generic;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class ListBannedIpsAck: Packet
    {
        [ProtoMember(1)]
        public List<PossibleBannedIp> BannedIps { get; set; }

        public ListBannedIpsAck()
        {
            PacketId = Protocols.LISTBANNEDIPS_ACK;
        }
    }
}
