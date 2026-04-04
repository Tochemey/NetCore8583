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
using System.Runtime.CompilerServices;

namespace NetCore8583.Extensions
{
    /// <summary>
    ///     High-performance 128-bit bitmap using UInt128 for ISO8583 field presence tracking.
    ///     Significantly faster than BitArray for fixed-size bitmaps.
    /// </summary>
    public struct Bitmap128
    {
        private UInt128 _bits;

        /// <summary>
        ///     Gets or sets individual bits by index (0-127).
        /// </summary>
        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index is < 0 or >= 128)
                    throw new IndexOutOfRangeException($"Bit index must be 0-127, got {index}");
                return (_bits & ((UInt128)1 << index)) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index is < 0 or >= 128)
                    throw new IndexOutOfRangeException($"Bit index must be 0-127, got {index}");
                if (value)
                    _bits |= (UInt128)1 << index;
                else
                    _bits &= ~((UInt128)1 << index);
            }
        }

        /// <summary>
        ///     Sets a bit at the specified index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value)
        {
            this[index] = value;
        }

        /// <summary>
        ///     Gets a bit at the specified index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index)
        {
            return this[index];
        }

        /// <summary>
        ///     Clears all bits (sets to 0).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _bits = 0;
        }

        /// <summary>
        ///     Returns true if any bit is set.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any()
        {
            return _bits != 0;
        }

        /// <summary>
        ///     Counts the number of set bits.
        /// </summary>
        public int PopCount()
        {
            return System.Numerics.BitOperations.PopCount((ulong)(_bits >> 64)) +
                   System.Numerics.BitOperations.PopCount((ulong)_bits);
        }

        /// <summary>
        /// Performs bitwise OR with another bitmap (in-place).
        /// </summary>
        /// <param name="other">The other bitmap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Or(Bitmap128 other)
        {
            _bits |= other._bits;
        }

        /// <summary>
        /// Performs bitwise AND with another bitmap (in-place).
        /// </summary>
        /// <param name="other">The other bitmap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void And(Bitmap128 other)
        {
            _bits &= other._bits;
        }

        /// <summary>
        /// Performs bitwise XOR with another bitmap (in-place).
        /// </summary>
        /// <param name="other">The other bitmap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Xor(Bitmap128 other)
        {
            _bits ^= other._bits;
        }

        /// <summary>
        ///     Returns the underlying UInt128 value.
        /// </summary>
        public UInt128 ToUInt128() => _bits;

        /// <summary>
        ///     Creates a Bitmap128 from a UInt128 value.
        /// </summary>
        public static Bitmap128 FromUInt128(UInt128 value) => new Bitmap128 { _bits = value };
    }
}
