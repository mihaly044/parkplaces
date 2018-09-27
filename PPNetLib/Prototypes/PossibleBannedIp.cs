using System;
using ProtoBuf;

namespace PPNetLib.Prototypes
{
    [ProtoContract]
    public class PossibleBannedIp
    {

        [ProtoMember(1)]
        public string IpPort { get; set; }

        [ProtoMember(2)]
        public int Tries { get; set; }

        [ProtoMember(3)]
        public DateTime Date { get; set; }

        public PossibleBannedIp()
        {
            
        }

        public PossibleBannedIp(string ipPort = "")
        {
            Date = DateTime.Now;
            IpPort = ipPort;
            Tries = 0;
        }

        public void Try()
        {
            Tries++;
        }

        public bool HasExpired()
        {
            return (DateTime.Now - Date).TotalMinutes >= 10;
        }
    }
}
