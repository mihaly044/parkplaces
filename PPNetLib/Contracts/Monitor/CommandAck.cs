using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class CommandAck : Packet
    {
        [ProtoMember(1)]
        public string Response { get; set; }

        public CommandAck()
        {
            PacketId = Protocols.COMMAND_ACK;
        }
    }
}
