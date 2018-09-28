using ProtoBuf;
namespace PPNetLib.Prototypes
{
    [ProtoContract]
    public class EncryptionKey
    {
        [ProtoMember(1)]
        public byte[] Salt { get; set; }

        [ProtoMember(2)]
        public string Password { get; set; }
    }
}
