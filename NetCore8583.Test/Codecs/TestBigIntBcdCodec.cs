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

using System.Numerics;
using NetCore8583.Codecs;
using Xunit;

namespace NetCore8583.Test.Codecs
{
    public class TestBigIntBcdCodec
    {
        private readonly BigIntBcdCodec _codec = new BigIntBcdCodec();

        [Fact]
        public void DecodeField_ParsesDecimalStringToBigInteger()
        {
            var result = _codec.DecodeField("12345");
            Assert.Equal(new BigInteger(12345), result);
        }

        [Fact]
        public void EncodeField_ReturnsBigIntegerAsString()
        {
            var result = _codec.EncodeField(new BigInteger(98765));
            Assert.Equal("98765", result);
        }

        [Fact]
        public void DecodeBinaryField_DecodesBcdBytesToBigInteger()
        {
            // BCD: 0x12 0x34 → "1234" (4 digits because length*2=4)
            var buf = new sbyte[] { 0x12, 0x34 };
            var result = _codec.DecodeBinaryField(buf, 0, 2);
            Assert.Equal(new BigInteger(1234), result);
        }

        [Fact]
        public void DecodeBinaryField_WithOffset()
        {
            var buf = new sbyte[] { 0x00, 0x12, 0x34 };
            var result = _codec.DecodeBinaryField(buf, 1, 2);
            Assert.Equal(new BigInteger(1234), result);
        }

        [Fact]
        public void EncodeBinaryField_EvenDigitCount()
        {
            // 1234 → "1234" (4 chars, even) → { 0x12, 0x34 }
            var result = _codec.EncodeBinaryField(new BigInteger(1234));
            Assert.Equal(new sbyte[] { 0x12, 0x34 }, result);
        }

        [Fact]
        public void EncodeBinaryField_OddDigitCount_PadsLeadingZero()
        {
            // 123 → "123" (3 chars, odd) → buf[0]=0x01, buf[1]=0x23
            var result = _codec.EncodeBinaryField(new BigInteger(123));
            Assert.Equal(new sbyte[] { 0x01, 0x23 }, result);
        }

        [Fact]
        public void RoundTrip_EncodeDecodeBinary()
        {
            var original = new BigInteger(567890);
            var encoded = _codec.EncodeBinaryField(original);
            var decoded = _codec.DecodeBinaryField(encoded, 0, encoded.Length);
            Assert.Equal(original, decoded);
        }
    }
}
