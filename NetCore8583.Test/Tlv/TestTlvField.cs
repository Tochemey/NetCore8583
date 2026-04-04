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
using NetCore8583.Tlv;
using Xunit;

namespace NetCore8583.Test.Tlv
{
    public class TestTlvField
    {
        private readonly TlvField _field = new();

        [Fact]
        public void DecodeFieldFromHexString()
        {
            var hex = "9F270180" + "82021980";
            var result = _field.DecodeField(hex);

            var tags = Assert.IsAssignableFrom<IReadOnlyList<TlvTag>>(result);
            Assert.Equal(2, tags.Count);
            Assert.Equal("9F27", tags[0].Tag);
            Assert.Equal("82", tags[1].Tag);
        }

        [Fact]
        public void EncodeFieldToHexString()
        {
            IReadOnlyList<TlvTag> tags = new List<TlvTag>
            {
                new("9F27", 1, new byte[] { 0x80 }),
                new("82", 2, new byte[] { 0x19, 0x80 })
            };

            var hex = _field.EncodeField(tags);

            Assert.Equal("9F270180" + "82021980", hex);
        }

        [Fact]
        public void DecodeBinaryField()
        {
            var raw = new sbyte[] { unchecked((sbyte)0x9F), 0x27, 0x01, unchecked((sbyte)0x80) };
            var result = _field.DecodeBinaryField(raw, 0, raw.Length);

            var tags = Assert.IsAssignableFrom<IReadOnlyList<TlvTag>>(result);
            Assert.Single(tags);
            Assert.Equal("9F27", tags[0].Tag);
        }

        [Fact]
        public void DecodeBinaryFieldWithOffset()
        {
            var raw = new sbyte[] { 0x00, 0x00, unchecked((sbyte)0x82), 0x02, 0x19, unchecked((sbyte)0x80) };
            var result = _field.DecodeBinaryField(raw, 2, 4);

            var tags = Assert.IsAssignableFrom<IReadOnlyList<TlvTag>>(result);
            Assert.Single(tags);
            Assert.Equal("82", tags[0].Tag);
        }

        [Fact]
        public void EncodeBinaryField()
        {
            IReadOnlyList<TlvTag> tags = new List<TlvTag>
            {
                new("82", 2, new byte[] { 0x19, 0x80 })
            };

            var result = _field.EncodeBinaryField(tags);

            Assert.Equal(4, result.Length);
            Assert.Equal(unchecked((sbyte)0x82), result[0]);
            Assert.Equal((sbyte)0x02, result[1]);
            Assert.Equal((sbyte)0x19, result[2]);
            Assert.Equal(unchecked((sbyte)0x80), result[3]);
        }

        [Fact]
        public void DecodeEmptyStringReturnsEmptyList()
        {
            var result = _field.DecodeField("");
            var tags = Assert.IsAssignableFrom<IReadOnlyList<TlvTag>>(result);
            Assert.Empty(tags);
        }

        [Fact]
        public void EncodeEmptyListReturnsEmptyString()
        {
            var result = _field.EncodeField(new List<TlvTag>());
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void DecodeBinaryZeroLengthReturnsEmptyList()
        {
            var result = _field.DecodeBinaryField(Array.Empty<sbyte>(), 0, 0);
            var tags = Assert.IsAssignableFrom<IReadOnlyList<TlvTag>>(result);
            Assert.Empty(tags);
        }

        [Fact]
        public void EncodeBinaryEmptyReturnsEmptyArray()
        {
            var result = _field.EncodeBinaryField(new List<TlvTag>());
            Assert.Empty(result);
        }
    }
}
