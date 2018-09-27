using System.Linq;

namespace PPNetLib.Extensions
{
    public static class Generic
    {
        public static byte ComputeAdditionChecksum(this byte[] data)
        {
            var longSum = data.Sum(x => (long)x);
            return unchecked((byte)longSum);
        }
    }
}
