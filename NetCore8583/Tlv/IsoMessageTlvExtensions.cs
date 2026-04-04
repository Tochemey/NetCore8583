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
using NetCore8583.Extensions;

namespace NetCore8583.Tlv
{
    /// <summary>
    /// Extension methods on <see cref="IsoMessage"/> for convenient access to field 55 (or any TLV-bearing field) as parsed <see cref="TlvTag"/> objects.
    /// </summary>
    public static class IsoMessageTlvExtensions
    {
        private const int DefaultTlvField = 55;

        /// <summary>
        /// Retrieves field 55 as a parsed list of <see cref="TlvTag"/> objects.
        /// Works both when a <see cref="TlvField"/> encoder is registered (object is already parsed)
        /// and when the raw value is a byte array or hex string (parses on-the-fly).
        /// </summary>
        /// <param name="message">The ISO message.</param>
        /// <param name="fieldNumber">The field number containing TLV data (default 55).</param>
        /// <returns>The parsed TLV tags, or null if the field is not present.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<TlvTag> GetTlvTags(this IsoMessage message, int fieldNumber = DefaultTlvField)
        {
            var isoValue = message.GetField(fieldNumber);
            if (isoValue == null)
                return null;

            var obj = isoValue.Value;

            if (obj is IReadOnlyList<TlvTag> tags)
                return tags;

            if (obj is byte[] bytes)
                return TlvParser.Parse(bytes);

            if (obj is sbyte[] signed)
                return TlvParser.Parse(signed.ToUint8());

            if (obj is string hex && !string.IsNullOrEmpty(hex))
                return TlvParser.Parse(Convert.FromHexString(hex));

            return null;
        }

        /// <summary>
        /// Sets field 55 with the given TLV tags, encoding them as BER-TLV.
        /// Requires a <see cref="TlvField"/> encoder to be registered on the <see cref="MessageFactory{T}"/> for the target field.
        /// </summary>
        /// <param name="message">The ISO message.</param>
        /// <param name="tags">The TLV tags to set.</param>
        /// <param name="encoder">The <see cref="TlvField"/> custom encoder. If null, a default instance is used.</param>
        /// <param name="isoType">The ISO type for the field (default LLLBIN for field 55).</param>
        /// <param name="fieldNumber">The field number (default 55).</param>
        /// <returns>The message for chaining.</returns>
        public static IsoMessage SetTlvTags(
            this IsoMessage message,
            IReadOnlyList<TlvTag> tags,
            TlvField encoder = null,
            IsoType isoType = IsoType.LLLBIN,
            int fieldNumber = DefaultTlvField)
        {
            encoder ??= new TlvField();

            var builder = new TlvBuilder();
            foreach (var tag in tags)
                builder.AddTag(tag);

            var data = builder.Build();
            message.SetValue(fieldNumber, tags, encoder, isoType, data.Length);
            return message;
        }

        /// <summary>
        /// Retrieves the raw TLV-encoded bytes from field 55 (or another TLV field).
        /// </summary>
        /// <param name="message">The ISO message.</param>
        /// <param name="fieldNumber">The field number (default 55).</param>
        /// <returns>The raw TLV bytes, or null if the field is not present.</returns>
        public static byte[] GetTlvBytes(this IsoMessage message, int fieldNumber = DefaultTlvField)
        {
            var isoValue = message.GetField(fieldNumber);
            if (isoValue == null)
                return null;

            var obj = isoValue.Value;

            if (obj is byte[] bytes)
                return bytes;

            if (obj is sbyte[] signed)
                return signed.ToUint8();

            if (obj is IReadOnlyList<TlvTag> tags)
            {
                var builder = new TlvBuilder();
                foreach (var tag in tags)
                    builder.AddTag(tag);
                return builder.Build();
            }

            if (obj is string hex && !string.IsNullOrEmpty(hex))
                return Convert.FromHexString(hex);

            return null;
        }

        /// <summary>
        /// Finds a specific TLV tag by its hex tag identifier within field 55.
        /// </summary>
        /// <param name="message">The ISO message.</param>
        /// <param name="tag">The tag hex string to find (e.g. "9F26").</param>
        /// <param name="fieldNumber">The field number (default 55).</param>
        /// <returns>The matching <see cref="TlvTag"/>, or null if not found.</returns>
        public static TlvTag FindTlvTag(this IsoMessage message, string tag, int fieldNumber = DefaultTlvField)
        {
            var tags = message.GetTlvTags(fieldNumber);
            return tags?.FirstOrDefault(t => string.Equals(t.Tag, tag, StringComparison.OrdinalIgnoreCase));
        }
    }
}
