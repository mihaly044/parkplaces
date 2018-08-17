﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PPNetLib.Extensions
{
    public static class Generic
    {
        public static byte ComputeAdditionChecksum(this byte[] data)
        {
            long longSum = data.Sum(x => (long)x);
            return unchecked((byte)longSum);
        }
    }
}
