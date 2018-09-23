using System;

namespace PPServer
{
    static class Program
    {
       static void Main(string[] args)
       {
            Server server;

            if (args.Length > 0 && args[0].Contains("nohttp"))
            {
                server = new Server(false);
            }
            else
            {
                server = new Server();
            }

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
