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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NetCore8583.Extensions;

namespace NetCore8583.Tlv
{
    /// <summary>
    /// An <see cref="ICustomBinaryField"/> implementation that encodes/decodes field values as BER-TLV data.
    /// Register an instance with <see cref="MessageFactory{T}.SetCustomField"/> for field 55 (or any other TLV-bearing field)
    /// so that the field's object value is an <see cref="IReadOnlyList{TlvTag}"/> instead of raw bytes.
    /// </summary>
    public sealed class TlvField : ICustomBinaryField
    {
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object DecodeField(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<TlvTag>();

            var bytes = Convert.FromHexString(value);
            return TlvParser.Parse(bytes);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string EncodeField(object value)
        {
            var tags = CastToTags(value);
            if (tags == null || tags.Count == 0)
                return string.Empty;

            var builder = new TlvBuilder();
            foreach (var tag in tags)
                builder.AddTag(tag);

            return Convert.ToHexString(builder.Build());
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object DecodeBinaryField(sbyte[] bytes, int offset, int length)
        {
            if (length <= 0)
                return Array.Empty<TlvTag>();

            ReadOnlySpan<byte> unsigned = MemoryMarshal.Cast<sbyte, byte>(bytes.AsSpan(offset, length));
            return TlvParser.Parse(unsigned);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte[] EncodeBinaryField(object value)
        {
            var tags = CastToTags(value);
            if (tags == null || tags.Count == 0)
                return Array.Empty<sbyte>();

            var builder = new TlvBuilder();
            foreach (var tag in tags)
                builder.AddTag(tag);

            return builder.Build().ToInt8();
        }

        private static IReadOnlyList<TlvTag> CastToTags(object value)
        {
            return value switch
            {
                IReadOnlyList<TlvTag> list => list,
                IEnumerable<TlvTag> enumerable => enumerable.ToList(),
                _ => null
            };
        }
    }
}
