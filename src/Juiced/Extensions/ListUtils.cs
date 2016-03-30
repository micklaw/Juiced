using System;
using System.Collections.Generic;

namespace Juiced
{
    /// <summary>
    /// Entensions for generic list
    /// </summary>
    internal static class ListUtils
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Get a random from the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                return default(T);
            }
            return list[_random.Next(0, list.Count)];
        }
    }
}
