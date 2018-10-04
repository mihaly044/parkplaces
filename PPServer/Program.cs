using System;
using CommandLine;
using PPServer.CommandLine;

namespace PPServer
{
    static class Program
    {
       static void Main(string[] args)
       {
            // Setting up the server
            var server = new Server();

            // Redirect console output
            var writer = new ConsoleWriter();
            Console.SetOut(writer);
            server.SetWriter(writer);

            // Parse command line arguments
            var useHttp = true;
            Parser.Default.ParseArguments<ServerOptions>(args)
                .WithParsed<ServerOptions>(o => {
                    if (o.LimitZones > 0)
                        server.LimitZones(o.LimitZones);

                    if (o.NoHttpServer)
                        useHttp = false;
                });

            if (useHttp)
                server.SetupHttpServer();

            // Start the server
            server.Listen();
            server.LoadData();
        }
    }
}
