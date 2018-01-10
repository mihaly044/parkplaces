using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParkPlaces.Utils
{
    public static class NetUtil
    {
        // TODO: Make it async so that it wont block the calling thread
        /// <summary>
        /// Downloads a string from a remote resource
        /// </summary>
        /// <param name="uri">URI of the remote resource</param>
        /// <returns>The downloaded content</returns>
        public static string GetStringFromUrl(string uri)
        {
            return new WebClient().DownloadString(uri);
        }
    }
}
