using System;
using System.Collections.Generic;
using System.Linq;

namespace Juiced
{
    /// <summary>
    /// Entensions for generic list
    /// </summary>
    internal static class ListUtils
    {
        /// <summary>
        /// Randomize the order of the list, preserving the original list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsRandom<T>(this IList<T> list)
        {
            int[] indexes = Enumerable.Range(0, list.Count).ToArray();
            Random generator = new Random();

            for (int i = 0; i < list.Count; ++i)
            {
                int position = generator.Next(i, list.Count);

                yield return list[indexes[position]];

                indexes[position] = indexes[i];
            }
        }

        private static Random random = new Random();

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
            return list[random.Next(0, list.Count)];
        }

        /// <summary>
        /// Shuffle the list order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list.Count <= 1)
            {
                return; // nothing to do 
            }

            for (int i = 0; i < list.Count; i++)
            {
                int newIndex = random.Next(0, list.Count);

                // swap the two elements over 
                T x = list[i];
                list[i] = list[newIndex];
                list[newIndex] = x;
            }
        }

        /// <summary>
        /// Add to a list if it is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="addItem"></param>
        public static void AddIfNotNull<T>(this IList<T> items, T addItem)
        {
            if (addItem == null)
            {
                return;
            }

            items.Add(addItem);
        }

        /// <summary>
        /// Add to a list if it is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="addItem"></param>
        public static void AddIfNotNull(this IList<string> items, string addItem)
        {
            if (string.IsNullOrWhiteSpace(addItem))
            {
                return;
            }

            items.Add(addItem);
        }

        public static bool HasContent<T>(this IEnumerable<T> items)
        {
            return (items != null && items.Any());
        }
    }
}
