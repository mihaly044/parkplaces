using System;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using PPNetLib.Prototypes;

namespace PPServer.Http
{
    class DtoController : WebApiController
    {
        public DtoController() : base()
        {
        }

        private bool InBounds(double pointLat, double pointLong, double boundsNElat, double boundsNElong, double boundsSWlat, double boundsSWlong)
        {
            bool eastBound = pointLong < boundsNElong;
            bool westBound = pointLong > boundsSWlong;

            bool inLong = false, inLat = false;

            if (boundsNElong < boundsSWlong)
            {
                inLong = eastBound || westBound;
            }
            else
            {
                inLong = eastBound && westBound;
            }

            inLat = pointLat > boundsSWlat && pointLat < boundsNElat;
            return inLat && inLong;
        }

        /*e.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            try
            {
                e.Response.Headers.Add("Content-type", "application/json");

                var path = System.Web.HttpUtility.UrlDecode(e.Request.Path);
                var request = Request.FromJson(path.Substring(path.IndexOf("=") + 1));

                var zones = new List<string>();

                var inBounds = false;
                foreach(var zone in Server.Dto.Zones)
                {
                    foreach (var point in zone.Geometry)
                    {
                        if (InBounds(point.Lat, point.Lng, request.North, request.East, request.South, request.West))
                        {
                            inBounds = true;
                            break;
                        }
                    }

                    if (inBounds)
                    {
                        zones.Add(JsonConvert.SerializeObject(zone, Converter.Settings));
                        inBounds = false;
                    }
                }

                using (var writer = new StreamWriter(e.Response.OutputStream))
                {
                    writer.Write($"{{ \"type\":\"ZoneCollection\", \"zones\": [{string.Join(",",zones.ToArray())}] }}");

                    ConsoleKit.Message(ConsoleKit.MessageType.DEBUG, "{0} zones served to {1}({2}) over HTTP\n",
                        zones.Count, e.Request.UserHostAddress, e.Request.UserHostName);
                }
            }
            catch (Exception ex)
            {
                if(!(ex is MySqlException))
                {
                    ConsoleKit.Message(ConsoleKit.MessageType.WARNING, "Invalid HTTP request from {0}({1}), {2}\n",
                        e.Request.UserHostAddress, e.Request.UserHostName, e.Request.Path);
                    e.Response.StatusCode = 500;
                    e.Response.Status = "500 Internal Server Error";
                }
            }
        }*/

        [WebApiHandler(HttpVerbs.Get, "/api/Hello/{north}/{east}/{south}/{west}")]
        public bool GetHello(WebServer server , HttpListenerContext context, double north, double east, double south, double west)
        {
            var zones = new List<string>();

            var inBounds = false;
            foreach (var zone in Server.Dto.Zones)
            {
                foreach (var point in zone.Geometry)
                {
                    if (InBounds(point.Lat, point.Lng, north, east, south, west))
                    {
                        inBounds = true;
                        break;
                    }
                }

                if (inBounds)
                {
                    zones.Add(JsonConvert.SerializeObject(zone, Converter.Settings));
                    inBounds = false;
                }
            }

            context.JsonResponse(JsonConvert.SerializeObject(zones));
            return true;
        }
    }
}
