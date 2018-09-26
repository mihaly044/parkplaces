using System;
using System.Collections.Generic;
using System.Linq;

namespace PPNetLib.Extensions
{
    public static class Generic
    {
        public static byte ComputeAdditionChecksum(this byte[] data)
        {
            long longSum = data.Sum(x => (long)x);
            return unchecked((byte)longSum);
        }

        /// <summary>
        /// Returns all the values from an enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
