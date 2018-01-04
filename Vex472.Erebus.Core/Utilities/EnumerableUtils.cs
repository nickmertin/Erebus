using System;
using System.Collections.Generic;

namespace Vex472.Erebus.Core.Utilities
{
    /// <summary>
    /// Provides utilities for interacting with types that implement interfaces in the <see cref="System.Collections.Generic"/> namespace.
    /// </summary>
    public static class EnumerableUtils
    {
        /// <summary>
        /// Adds the specified key paired with a new instance of <typeparamref name="TValue"/> to the given dictionary if it does not exist already, then returns the associated value.
        /// </summary>
        /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with <paramref name="key"/>.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new TValue());
            return dictionary[key];
        }

        /// <summary>
        /// Retrieves an enumerable of <typeparamref name="T"/> that contains a specified subset of <paramref name="list"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerable.</typeparam>
        /// <param name="list">The enumerable.</param>
        /// <param name="start">The starting index of the range.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>The range of elements.</returns>
        public static IEnumerable<T> Range<T>(this IEnumerable<T> list, int start, int count)
        {
            int i = 0;
            foreach (var e in list)
            {
                if (i++ < start)
                    continue;
                if (i > start + count)
                    break;
                yield return e;
            }
        }

        /// <summary>
        /// Gets whether or not the enumerable has elements in it.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerable.</typeparam>
        /// <param name="list">The enumerable</param>
        /// <returns>True if <paramref name="list"/> has elements in it; otherwise false.</returns>
        public static bool HasElements<T>(this IEnumerable<T> list) => list.GetEnumerator().MoveNext();

        /// <summary>
        /// Gets the zero-based index of the first occurrence of <paramref name="element"/> in the enumerable.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerable.</typeparam>
        /// <param name="list">The enumerable.</param>
        /// <param name="element">The element to look for.</param>
        /// <returns>The index of the element, or -1 if it is not in the enumerable.</returns>
        public static int FirstIndexOf<T>(this IEnumerable<T> list, T element)
        {
            int i = 0;
            foreach (var e in list)
            {
                if (e.Equals(element))
                    return i;
                ++i;
            }
            return -1;
        }

        /// <summary>
        /// Appends the specified element to the specified array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="array">The array to add the element to.</param>
        /// <param name="element">The element to add.</param>
        /// <returns>The array.</returns>
        public static T[] Append<T>(this T[] array, T element)
        {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = element;
            return array;
        }
    }
}