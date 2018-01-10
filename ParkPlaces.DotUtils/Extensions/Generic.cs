using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkPlaces.DotUtils.Extensions
{
    public static class Generic
    {
        public static bool IsDefault<T>(this T input)
        {
            return input.Equals(default(T));
        }
    }
}