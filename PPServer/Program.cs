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
                var input = Console.ReadLine();
                switch (input)
                {
                    case "q":
                        server.AnnounceShutdownAck(10);
                    break;
                }
            }
       }
    }
}
