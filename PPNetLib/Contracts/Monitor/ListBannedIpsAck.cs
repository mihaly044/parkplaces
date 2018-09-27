using PPNetLib.Prototypes;
using ProtoBuf;
using System.Collections.Generic;

namespace PPNetLib.Contracts.Monitor
{
    public class ListBannedIpsAck: Packet
    {
        public List<PossibleBannedIp> BannedIps { get; set; }

        public ListBannedIpsAck()
        {
            PacketId = Protocols.LISTBANNEDIPS_ACK;
        }
    }
}
