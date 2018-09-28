using ProtoBuf;

namespace PPNetLib.Contracts.Monitor
{
    [ProtoContract]
    public class CommandReq: Packet
    {
        [ProtoMember(1)]
        public string[] Command { get; set; }

        public CommandReq()
        {
            PacketId = Protocols.COMMAND_REQ;
        }
    }
}
