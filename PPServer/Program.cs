using System;

namespace PPServer
{
    static class Program
    {
       static void Main(string[] args)
       {
            var writer = new ConsoleWriter();
            //Console.SetOut(writer);

            var server = new Server(writer);
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
