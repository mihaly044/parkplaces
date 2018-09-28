using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace PPServer.CommandLine
{
    [Verb("echo", HelpText = "Echo a text back to the client")]
    public class Echo
    {
        [Option('t', "text", Required = true, HelpText = "The text to be echoed back")]
        public string Text { get; set; }
    }

    [Verb("kickuser", HelpText = "Kick an user from the server")]
    public class KickUser
    {
        [Option('u', "username", Required = true, HelpText = "The name of the user to disconnected")]
        public string UserName { get; set; }
    }

    [Verb("banip", HelpText = "Ban an IP from accessing the server")]
    public class BanIp
    {
        [Option('i', "ipaddress", Required = true, HelpText = "The IP to be banned")]
        public string IPAddress { get; set; }
    }

    [Verb("unbanip", HelpText = "Unban an IP from accessing the server")]
    public class UnbanIp
    {
        [Option('i', "ipaddress", Required = true, HelpText = "The IP to be banned")]
        public string IPAddress { get; set; }
    }

    [Verb("shutdown", HelpText = "Unban an IP from accessing the server")]
    public class Shutdown
    {
        [Option('s', "seconds", Required = false, HelpText = "Shuts down the application within S seconds.")]
        public int Seconds { get; set; }
    }

    [Verb("get", HelpText = "Get information about the server and data stored")]
    public class Get
    {
        [Option('o', "online-users", Required = false, HelpText = "Return a list of online users")]
        public bool OnlineUsers { get; set; }

        [Option('z', "zone-count", Required = false, HelpText = "Return the count of all zones")]
        public bool ZoneCount { get; set; }

        [Option('m', "memusage", Required = false, HelpText = "Return the current memory usage of the server")]
        public bool MemUsage { get; set; }
    }
}
