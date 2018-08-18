using System;

namespace PPServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            server.Listen();
            while(true) Console.ReadLine();
        }
    }
}
