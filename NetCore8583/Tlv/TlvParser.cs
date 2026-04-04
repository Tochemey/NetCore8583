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

namespace NetCore8583.Tlv
{
    /// <summary>
    /// Parses raw byte data encoded in BER-TLV format (as used by EMV / ISO 7816-4) into a list of <see cref="TlvTag"/> objects.
    /// Handles single-byte and multi-byte tags as well as single-byte and multi-byte (definite) lengths.
    /// </summary>
    public static class TlvParser
    {
        /// <summary>
        /// Parses a BER-TLV encoded byte array into an ordered list of <see cref="TlvTag"/> data objects.
        /// </summary>
        /// <param name="data">The raw TLV-encoded bytes.</param>
        /// <returns>An ordered, read-only list of parsed TLV tags.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        /// <exception cref="FormatException">Thrown when the data contains malformed TLV structures.</exception>
        public static IReadOnlyList<TlvTag> Parse(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return Parse(data.AsSpan());
        }

        /// <summary>
        /// Parses a BER-TLV encoded byte span into an ordered list of <see cref="TlvTag"/> data objects.
        /// </summary>
        /// <param name="data">The raw TLV-encoded bytes.</param>
        /// <returns>An ordered, read-only list of parsed TLV tags.</returns>
        /// <exception cref="FormatException">Thrown when the data contains malformed TLV structures.</exception>
        public static IReadOnlyList<TlvTag> Parse(ReadOnlySpan<byte> data)
        {
            var tags = new List<TlvTag>();
            var offset = 0;

            while (offset < data.Length)
            {
                // Skip padding bytes (0x00 and 0xFF are inter-TLV padding per EMV Book 3)
                if (data[offset] == 0x00 || data[offset] == 0xFF)
                {
                    offset++;
                    continue;
                }

                var (tag, tagLength) = ReadTag(data, offset);
                offset += tagLength;

                if (offset >= data.Length)
                    throw new FormatException($"TLV data truncated after tag {tag} at offset {offset - tagLength}.");

                var (valueLength, lengthFieldSize) = ReadLength(data, offset);
                offset += lengthFieldSize;

                if (offset + valueLength > data.Length)
                    throw new FormatException(
                        $"TLV value for tag {tag} at offset {offset} declares length {valueLength} but only {data.Length - offset} bytes remain.");

                var value = data.Slice(offset, valueLength).ToArray();
                offset += valueLength;

                var description = EmvTags.GetDescription(tag);
                tags.Add(new TlvTag(tag, valueLength, value, description));
            }

            return tags;
        }

        /// <summary>
        /// Reads a BER-TLV tag at the given offset.
        /// Per ISO 8825-1 / EMV v4.3 Book 3:
        ///   - If bits 5–1 of the first byte are all 1 (lower 5 bits == 0x1F), the tag is multi-byte.
        ///   - Subsequent bytes are part of the tag while bit 8 is set; the last tag byte has bit 8 cleared.
        /// </summary>
        /// <returns>A tuple of (tag hex string, number of bytes consumed).</returns>
        internal static (string Tag, int BytesConsumed) ReadTag(ReadOnlySpan<byte> data, int offset)
        {
            if (offset >= data.Length)
                throw new FormatException("Unexpected end of data while reading TLV tag.");

            var start = offset;
            var firstByte = data[offset++];

            if ((firstByte & 0x1F) == 0x1F)
            {
                // Multi-byte tag
                while (offset < data.Length)
                {
                    var b = data[offset++];
                    if ((b & 0x80) == 0)
                        break; // last byte of multi-byte tag

                    if (offset >= data.Length)
                        throw new FormatException("Unexpected end of data in multi-byte TLV tag.");
                }
            }

            var tagLength = offset - start;
            var tagHex = Convert.ToHexString(data.Slice(start, tagLength));
            return (tagHex, tagLength);
        }

        /// <summary>
        /// Reads a BER-TLV length at the given offset.
        /// Per ISO 8825-1:
        ///   - If the first byte is 0x00–0x7F, it is the length (short form).
        ///   - If bit 8 is set, bits 7–1 indicate how many subsequent bytes encode the length (long form, definite).
        ///   - Indefinite form (0x80) is not used in EMV and is rejected.
        /// </summary>
        /// <returns>A tuple of (decoded length, number of bytes consumed by the length field).</returns>
        internal static (int Length, int BytesConsumed) ReadLength(ReadOnlySpan<byte> data, int offset)
        {
            if (offset >= data.Length)
                throw new FormatException("Unexpected end of data while reading TLV length.");

            var firstByte = data[offset++];

            // Short form: single byte, 0x00–0x7F
            if ((firstByte & 0x80) == 0)
                return (firstByte, 1);

            // Long form: first byte encodes the number of subsequent length bytes
            var numLenBytes = firstByte & 0x7F;

            if (numLenBytes == 0)
                throw new FormatException("Indefinite-length encoding is not supported in EMV BER-TLV.");

            if (numLenBytes > 4)
                throw new FormatException($"TLV length field is too large ({numLenBytes} bytes); maximum supported is 4.");

            if (offset + numLenBytes > data.Length)
                throw new FormatException("Unexpected end of data while reading multi-byte TLV length.");

            var length = 0;
            for (var i = 0; i < numLenBytes; i++)
            {
                length = (length << 8) | data[offset++];
            }

            return (length, 1 + numLenBytes);
        }
    }
}
