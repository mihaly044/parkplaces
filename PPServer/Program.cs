using System;
using System.Threading.Tasks;
using CommandLine;
using PPServer.CommandLine;
using PPServer.Http;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;

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
                RunHttp();

            // Start the server
            server.Listen();
            server.LoadData();

            
        }

        public static async void RunHttp()
        {
            var server = new WebServer("https://parkplaces.pw:11001", RoutingStrategy.Regex);
            server.RegisterModule(new WebApiModule());
            server.Module<WebApiModule>().RegisterController<DtoController>();

            await Task.Run( () => server.RunAsync());
        }
    }
}
