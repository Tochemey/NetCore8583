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
    /// Represents a single BER-TLV data object with tag identifier, length, value, and optional human-readable description.
    /// </summary>
    /// <param name="Tag">The tag identifier as an uppercase hex string (e.g. "9F26", "82").</param>
    /// <param name="Length">The length of the value in bytes.</param>
    /// <param name="Value">The raw value bytes.</param>
    /// <param name="Description">An optional human-readable description of the tag (e.g. "Application Cryptogram").</param>
    public sealed record TlvTag(string Tag, int Length, byte[] Value, string Description = null)
    {
        private byte[] _tagBytes;

        /// <summary>Returns the tag bytes decoded from the hex <see cref="Tag"/> string. The result is cached after the first call.</summary>
        public byte[] TagBytes => _tagBytes ??= Convert.FromHexString(Tag);

        /// <summary>True when the tag represents a constructed data object (bit 6 of the first tag byte is set per ISO 8825-1).</summary>
        public bool IsConstructed
        {
            get
            {
                if (Tag.Length < 2) return false;
                var firstNibble = HexCharToInt(Tag[0]);
                return (firstNibble & 0x02) != 0;
            }
        }

        /// <summary>Parses the value of a constructed tag as a sequence of nested TLV objects.</summary>
        /// <returns>The nested TLV tags, or an empty list if the value contains no valid TLV data.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this tag is not constructed.</exception>
        public IReadOnlyList<TlvTag> GetNestedTags()
        {
            if (!IsConstructed)
                throw new InvalidOperationException($"Tag {Tag} is not a constructed tag.");
            return TlvParser.Parse(Value);
        }

        /// <inheritdoc />
        public bool Equals(TlvTag other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Tag, other.Tag, StringComparison.OrdinalIgnoreCase)
                   && Length == other.Length
                   && ValueEquals(Value, other.Value);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Tag, StringComparer.OrdinalIgnoreCase);
            hash.Add(Length);
            if (Value != null)
                hash.AddBytes(Value);
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var hex = Value != null ? Convert.ToHexString(Value) : "";
            var desc = Description != null ? $" ({Description})" : "";
            return $"[{Tag}] len={Length} val={hex}{desc}";
        }

        private static bool ValueEquals(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.AsSpan().SequenceEqual(b);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int HexCharToInt(char c) => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => 0
        };
    }
}
