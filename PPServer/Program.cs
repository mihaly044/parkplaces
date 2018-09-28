using System;
using CommandLine;
using PPServer.CommandLine;

namespace PPServer
{
    static class Program
    {
       static void Main(string[] args)
       {
            var writer = new ConsoleWriter();
            Console.SetOut(writer);
            var server = new Server(writer);

            Parser.Default.ParseArguments<ServerOptions>(args)
                .WithParsed<ServerOptions>(o=> {
                    if(o.LimitZones > 0)
                        server.LimitZones(o.LimitZones);
                });

            server.Listen();
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
