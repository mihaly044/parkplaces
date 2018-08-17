﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPNetLib
{
    public static class Protocols
    {
        public const int LOGIN_REQ = 0x0001;
        public const int LOGIN_ACK = 0x0002;
        public const int ZONECOUNT_REQ = 0x0003;
        public const int ZONECOUNT_ACK = 0x0004;
        public const int ZONELIST_REQ = 0x0005;
        public const int ZONELIST_ACK = 0x0006;
    }
}
