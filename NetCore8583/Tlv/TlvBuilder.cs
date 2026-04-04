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
using System.IO;

namespace NetCore8583.Tlv
{
    /// <summary>
    /// Fluent builder for constructing BER-TLV encoded byte sequences.
    /// Supports single-byte and multi-byte tags, and encodes lengths using the minimal BER-TLV form.
    /// </summary>
    public sealed class TlvBuilder
    {
        private readonly List<(byte[] TagBytes, byte[] Value)> _entries = new();

        /// <summary>
        /// Adds a TLV data object with the given tag and value.
        /// </summary>
        /// <param name="tag">The tag as a hex string (e.g. "9F26", "82"). Must be a valid BER-TLV tag.</param>
        /// <param name="value">The raw value bytes for this tag.</param>
        /// <returns>This builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is null, empty, or not valid hex.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public TlvBuilder AddTag(string tag, byte[] value)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Tag must not be null or empty.", nameof(tag));
            ArgumentNullException.ThrowIfNull(value);

            var tagBytes = Convert.FromHexString(tag);
            if (tagBytes.Length == 0)
                throw new ArgumentException("Tag must decode to at least one byte.", nameof(tag));

            _entries.Add((tagBytes, value));
            return this;
        }

        /// <summary>
        /// Adds a TLV data object with the tag and value specified as raw bytes.
        /// The caller must not mutate the arrays after passing them to the builder.
        /// </summary>
        /// <param name="tagBytes">The raw tag bytes.</param>
        /// <param name="value">The raw value bytes.</param>
        /// <returns>This builder for chaining.</returns>
        public TlvBuilder AddTag(byte[] tagBytes, byte[] value)
        {
            ArgumentNullException.ThrowIfNull(tagBytes);
            ArgumentNullException.ThrowIfNull(value);
            if (tagBytes.Length == 0)
                throw new ArgumentException("Tag must have at least one byte.", nameof(tagBytes));

            _entries.Add((tagBytes, value));
            return this;
        }

        /// <summary>
        /// Adds a pre-parsed <see cref="TlvTag"/> to the builder.
        /// </summary>
        /// <param name="tlvTag">The TLV tag to add.</param>
        /// <returns>This builder for chaining.</returns>
        public TlvBuilder AddTag(TlvTag tlvTag)
        {
            ArgumentNullException.ThrowIfNull(tlvTag);
            return AddTag(tlvTag.Tag, tlvTag.Value);
        }

        /// <summary>
        /// Builds the BER-TLV encoded byte sequence from all added tags.
        /// </summary>
        /// <returns>The concatenated TLV-encoded byte array.</returns>
        public byte[] Build()
        {
            using var stream = new MemoryStream();

            foreach (var (tagBytes, value) in _entries)
            {
                stream.Write(tagBytes, 0, tagBytes.Length);
                WriteLength(stream, value.Length);
                stream.Write(value, 0, value.Length);
            }

            return stream.ToArray();
        }

        /// <summary>
        /// Encodes a length value using BER-TLV definite-form encoding and writes it to the stream.
        /// Uses the minimal encoding: short form for lengths 0–127, long form for 128+.
        /// </summary>
        internal static void WriteLength(Stream stream, int length)
        {
            switch (length)
            {
                case < 0:
                    throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");
                case <= 0x7F:
                    stream.WriteByte((byte)length);
                    break;
                case <= 0xFF:
                    stream.WriteByte(0x81);
                    stream.WriteByte((byte)length);
                    break;
                case <= 0xFFFF:
                    stream.WriteByte(0x82);
                    stream.WriteByte((byte)(length >> 8));
                    stream.WriteByte((byte)(length & 0xFF));
                    break;
                case <= 0xFFFFFF:
                    stream.WriteByte(0x83);
                    stream.WriteByte((byte)(length >> 16));
                    stream.WriteByte((byte)((length >> 8) & 0xFF));
                    stream.WriteByte((byte)(length & 0xFF));
                    break;
                default:
                    stream.WriteByte(0x84);
                    stream.WriteByte((byte)(length >> 24));
                    stream.WriteByte((byte)((length >> 16) & 0xFF));
                    stream.WriteByte((byte)((length >> 8) & 0xFF));
                    stream.WriteByte((byte)(length & 0xFF));
                    break;
            }
        }
    }
}
