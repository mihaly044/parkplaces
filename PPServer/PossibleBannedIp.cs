using System;
using System.Collections.Generic;
using System.Text;

namespace PPServer
{
    public class PossibleBannedIp
    {
        public string IpPort { get; set; }
        public int Tries { get; private set; }
        public DateTime Date { get; private set; }

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
