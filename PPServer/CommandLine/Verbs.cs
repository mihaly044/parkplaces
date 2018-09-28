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
}
