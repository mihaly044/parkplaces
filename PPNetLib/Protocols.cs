using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPNetLib
{
    public enum Protocols
    {
        LOGIN_REQ = 0x0001,
        LOGIN_ACK,
        ZONECOUNT_REQ,
        ZONECOUNT_ACK,
        ZONELIST_REQ,
        ZONELIST_ACK,
        INSERTZONE_REQ,
        INSERTZONE_ACK,
        REMOVEZONE_REQ
    };
}
