using System;

namespace NetCore8583.Util
{
    public static class Arrays
    {
        /// <summary>
        ///     Fill fills an array with value from a starting position to a given ending position
        /// </summary>
        /// <param name="array">The array to fill</param>
        /// <param name="start">The starting position</param>
        /// <param name="count">The number of items to fill</param>
        /// <param name="value">The item to fill</param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Fill<T>(T[] array,
            int start,
            int count,
            T value)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (start + count >= array.Length) throw new ArgumentOutOfRangeException(nameof(count));
            for (var i = start; i < start + count; i++) array[i] = value;
        }
    }
}