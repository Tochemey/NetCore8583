// MIT License
//
// Copyright (c) 2020 - 2026 Arsene Tochemey Gandote
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace NetCore8583.Extensions
{
    /// <summary>Array utility methods used by the ISO 8583 library.</summary>
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