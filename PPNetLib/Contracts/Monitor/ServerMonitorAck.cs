﻿using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class ServerMonitorAck: Packet
    {
        [ProtoMember(1)]
        public string Output { get; set; }

        public ServerMonitorAck()
        {
            PacketId = Protocols.SERVERMONITOR_ACK;
        }
    }
}
