﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
// ReSharper disable RedundantCommaInEnumDeclaration

namespace PPNetLib
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
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
        REMOVEZONE_REQ,
        UPDATEZONE_REQ,
        REMOVEPOINT_REQ,
        INSERTPOINT_REQ,
        INSERTPOINT_ACK,
        UPDATEPOINT_REQ
    };
}
