using System;

namespace NetCore8583.Util
{
    public static class Arrays
    {
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