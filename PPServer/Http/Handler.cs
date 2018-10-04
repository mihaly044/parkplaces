using NHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using PPNetLib.Prototypes;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Collections.Specialized;
using PPNetLib;

namespace PPServer.Http
{
    public class Handler
    {
        private HttpServer _httpServer;
        private readonly int _httpPort;
        private readonly Server _server;
        private const int HttpServerPort = 8080;
        
        public Handler(Server server)
        {
            var configSect = ConfigurationManager.GetSection("ServerConfiguration") as NameValueCollection;
            _httpPort = configSect != null ? int.Parse(configSect["HttpServerPort"]) : HttpServerPort;
            _server = server;
        }

        public void Handle()
        {
            _httpServer = new HttpServer {EndPoint = new IPEndPoint(IPAddress.Loopback, _httpPort)};
            _httpServer.RequestReceived += _httpServer_RequestReceived;
            _httpServer.Start();
            ConsoleKit.Message(ConsoleKit.MessageType.INFO, "HTTP server listening on {0}:{1}\n", 
                _httpServer.EndPoint.Address, _httpServer.EndPoint.Port);
        }

        private bool InBounds(double pointLat, double pointLong, double boundsNElat, double boundsNElong, double boundsSWlat, double boundsSWlong)
        {
            bool eastBound = pointLong < boundsNElong;
            bool westBound = pointLong > boundsSWlong;

            bool inLong = false, inLat = false;

            if(boundsNElong < boundsSWlong)
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

        private void _httpServer_RequestReceived(object sender, HttpRequestEventArgs e)
        {
            e.Response.Headers.Add("Access-Control-Allow-Origin", "*");

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
        }
    }
}
