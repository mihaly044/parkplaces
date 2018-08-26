using System;
using System.Threading;
using System.Threading.Tasks;

namespace PPServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
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
