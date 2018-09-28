using System;
using CommandLine;
using PPServer.CommandLine;

namespace PPServer
{
    static class Program
    {
       static void Main(string[] args)
       {
            var server = new Server();
            var writer = new ConsoleWriter();
            Console.SetOut(writer);
            server.SetWriter(writer);

            var useHttp = true;

            Parser.Default.ParseArguments<ServerOptions>(args)
                .WithParsed<ServerOptions>(o => {
                    if (o.LimitZones > 0)
                        server.LimitZones(o.LimitZones);

                    if (o.NoHttpServer)
                        useHttp = false;
                });

            if(useHttp)
                server.SetupHttpServer();

            server.Listen();
            server.LoadData();

            while (true)
            {
                Console.Write("command > ");
                var input = Console.ReadLine().Split(' ');

                Parser.Default.ParseArguments<Echo, KickUser, BanIp, UnbanIp, Get, Shutdown>(input)
                .WithParsed<KickUser>(o =>
                {
                    var kickUser = server.Guard.GetAuthUserByName(o.UserName);
                    server.DisconnectUser(kickUser.IpPort);
                })
                .WithParsed<BanIp>(o =>
                {
                    server.Guard.BanIp(o.IPAddress);
                    server.DisconnectUser(o.IPAddress);
                })
                .WithParsed<UnbanIp>(o =>
                {
                    server.Guard.UnbanIp(o.IPAddress);
                })
                .WithParsed<Get>(o =>
                {
                    var get = "";
                    if (o.OnlineUsers)
                    {
                        var users = server.Guard.GetAuthUsers();
                        get = $"Online: {users.Count}[{string.Join(", ", users)}]";
                    }
                    else if (o.ZoneCount)
                    {
                        get = "Zone count: " + server.Dto.Zones.Count.ToString();
                    }
                    else if (o.MemUsage)
                    {
                        get = $"Total memory usage is {GC.GetTotalMemory(true) / 1024 / 1024} MB";
                    }
                    else
                    {
                        get = "Unknown value requested";
                    }
                    Console.WriteLine(get);
                })
                .WithParsed<Shutdown>(o =>
                {
                    if (o.Seconds > 0)
                        server.AnnounceShutdownAck(o.Seconds);
                    else
                        server.Shutdown();

                });
            }
       }
    }
}
