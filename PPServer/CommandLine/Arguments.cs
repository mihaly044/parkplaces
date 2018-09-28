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
}
