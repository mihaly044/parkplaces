using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPServer
{
    public class ConsoleWriterEventArgs : EventArgs
    {
        public string Value { get; private set; }
        public ConsoleWriterEventArgs(string value)
        {
            Value = value;
        }
    }

    public class ConsoleWriter : TextWriter
    {
        private TextWriter _writer;

        public ConsoleWriter()
        {
            _writer = Console.Out;
        }

        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public override void Write(string value)
        {
            if (WriteEvent != null) WriteEvent(this, new ConsoleWriterEventArgs(value));
            _writer.Write(value);
        }

        public override void WriteLine(string value)
        {
            if (WriteLineEvent != null) WriteLineEvent(this, new ConsoleWriterEventArgs(value));
            _writer.WriteLine(value);
        }

        public event EventHandler<ConsoleWriterEventArgs> WriteEvent;
        public event EventHandler<ConsoleWriterEventArgs> WriteLineEvent;
    }
}
