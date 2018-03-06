using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace ParkPlaces.Extensions
{
    public static class Generic
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static int GetExceptionNumber(this MySqlException exception)
        {
            var innerException = exception.InnerException as MySqlException;
            if (exception.Number == 0 && innerException != null)
            {
                return innerException.Number;
            }
            else
            {
                return exception.Number;
            }
        }
    }
}
