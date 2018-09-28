using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class CommandReq: Packet
    {
        public string[] Command { get; set; }

        public CommandReq()
        {
            PacketId = Protocols.COMMAND_REQ;
        }
    }
}
