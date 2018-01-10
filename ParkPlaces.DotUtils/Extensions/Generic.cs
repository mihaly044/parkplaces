using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        public static int ForEach<T>(this IEnumerable<T> input, Action<T> toRunAct)
        {
            int iterations = 0;
            if (input == null) return iterations;
            foreach (var put in input)
            {
                if (!put.Equals(default(T)))
                {
                    toRunAct(put);
                    iterations++;
                }
            }
            return iterations;
        }

        public static async Task<List<X>> ParallelForEachTaskAsync<T, X>(this IEnumerable<T> input, Func<T, Task<X>> toRunAct)
        {
            var lst = new List<Task<X>>();
            var res = new List<X>();

            if (input == null) return res;

            foreach (var put in input)
            {
                if (!put.Equals(default(X)))
                    lst.Add(toRunAct(put));
            }

            if (lst.Count > 0)
            {
                await Task.WhenAll(lst.ToArray());

                foreach (var rs in lst)
                {
                    res.Add(await rs);
                }
            }

            return res;
        }
    }
}