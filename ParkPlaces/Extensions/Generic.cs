using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParkPlaces.Extensions
{
    public static class Generic
    {
        /// <summary>
        /// Provides the same result as a SELECT DISTINCT 
        /// query
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Return the error number of a MySQL exception with
        /// taking the inner exception into account
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static int GetExceptionNumber(this MySqlException exception)
        {
            var innerException = exception.InnerException as MySqlException;
            if (exception.Number == 0 && innerException != null)
            {
                return innerException.Number;
            }
            return exception.Number;
        }
    }
}
