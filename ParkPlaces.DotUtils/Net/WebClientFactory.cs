using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParkPlaces.DotUtils.Net
{
    internal static class WebClientFactory
    {
        public static WebClient GetClient()
        {
            return new WebClient
            {
                Proxy = null,
                Encoding = Encoding.UTF8,
                Headers =
                {
                    ["Cache-Control"] = "max-age=0",
                    ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    ["User-agent"] = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0",
                    ["Accept-Language"] = "en-US,en;q=0.8"
                }
            };
        }
    }
}