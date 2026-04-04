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
using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Unit tests for <see cref="NumericParseInfo"/>.
    ///
    /// ASCII format: inherited from AlphaNumericFieldParseInfo - reads length chars as string.
    ///
    /// Binary format (BCD):
    ///   For length &lt; 19 digits: uses <see cref="Bcd.DecodeToLong"/>, returns a long value.
    ///   For length &gt;= 19 digits: uses <see cref="Bcd.DecodeToBigInteger"/>, returns a BigInteger.
    ///
    ///   Bytes consumed = length / 2 + length % 2 (rounded up for odd digit counts).
    /// </summary>
    public class TestNumericParseInfo
    {
        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

        // ═══════════════════════════════════════════════════════════════════════
        // Parse (ASCII) — inherited from AlphaNumericFieldParseInfo
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Parse_ReturnsCorrectValue()
        {
            var fpi = new NumericParseInfo(6);
            var val = fpi.Parse(1, Ascii("123456"), 0, null);
            Assert.Equal(IsoType.NUMERIC, val.Type);
            Assert.Equal("123456", val.ToString());
        }

        [Fact]
        public void Parse_WithLeadingZeros()
        {
            var fpi = new NumericParseInfo(6);
            var val = fpi.Parse(1, Ascii("000042"), 0, null);
            Assert.Equal("000042", val.ToString());
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new NumericParseInfo(4);
            var val = fpi.Parse(1, Ascii("XXXX1234"), 4, null);
            Assert.Equal("1234", val.ToString());
        }

        [Fact]
        public void Parse_NegativePosition_Throws()
        {
            var fpi = new NumericParseInfo(6);
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("123456"), -1, null));
        }

        [Fact]
        public void Parse_InsufficientData_Throws()
        {
            var fpi = new NumericParseInfo(10);
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("123"), 0, null));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary — short length (< 19 digits), returns long
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_SixDigits_ReturnsLong()
        {
            // BCD {0x12,0x34,0x56}, length=6 → 123456
            var fpi = new NumericParseInfo(6);
            var buf = new sbyte[] { 0x12, 0x34, 0x56 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.NUMERIC, val.Type);
            Assert.Equal(123456L, val.Value);
            Assert.Equal(6, val.Length);
        }

        [Fact]
        public void ParseBinary_FourDigits()
        {
            // BCD {0x12,0x34}, length=4 → 1234
            var fpi = new NumericParseInfo(4);
            var buf = new sbyte[] { 0x12, 0x34 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(1234L, val.Value);
        }

        [Fact]
        public void ParseBinary_TwoDigits()
        {
            // BCD {0x42}, length=2 → 42
            var fpi = new NumericParseInfo(2);
            var buf = new sbyte[] { 0x42 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(42L, val.Value);
        }

        [Fact]
        public void ParseBinary_AllZeros()
        {
            var fpi = new NumericParseInfo(6);
            var buf = new sbyte[] { 0x00, 0x00, 0x00 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(0L, val.Value);
        }

        [Fact]
        public void ParseBinary_WithOffset()
        {
            // 2 bytes garbage, then BCD {0x12,0x34,0x56} = 123456
            var fpi = new NumericParseInfo(6);
            var buf = new sbyte[] { 0x00, 0x00, 0x12, 0x34, 0x56 };
            var val = fpi.ParseBinary(1, buf, 2, null);
            Assert.Equal(123456L, val.Value);
        }

        [Fact]
        public void ParseBinary_EighteenDigits_ReturnsLong()
        {
            // 18-digit max for long: 9 BCD bytes
            var fpi = new NumericParseInfo(18);
            var buf = new sbyte[]
            {
                0x12, 0x34, 0x56, 0x78,
                unchecked((sbyte)0x90), 0x12, 0x34, 0x56, 0x78
            };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(123456789012345678L, val.Value);
        }

        [Fact]
        public void ParseBinary_NegativePosition_Throws()
        {
            var fpi = new NumericParseInfo(6);
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x12, 0x34, 0x56 }, -1, null));
        }

        [Fact]
        public void ParseBinary_InsufficientData_Throws()
        {
            // length=6 needs 3 bytes; check is pos + length/2 > buf.Length → 0+3 > 2 → throws
            var fpi = new NumericParseInfo(6);
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x12, 0x34 }, 0, null));
        }

        [Fact]
        public void ParseBinary_InsufficientDataFromOffset_Throws()
        {
            var fpi = new NumericParseInfo(6);
            // offset=3, buf.Length=5, needs 3 bytes → 3+3=6 > 5 → throws
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x00, 0x00, 0x00, 0x12, 0x34 }, 3, null));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary — long length (>= 19 digits), returns BigInteger
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_TwentyDigits_ReturnsBigInteger()
        {
            // 20-digit number: 10 BCD bytes
            // {0x12,0x34,0x56,0x78,0x90,0x12,0x34,0x56,0x78,0x90} → 12345678901234567890
            var fpi = new NumericParseInfo(20);
            var buf = new sbyte[]
            {
                0x12, 0x34, 0x56, 0x78,
                unchecked((sbyte)0x90), 0x12, 0x34, 0x56, 0x78,
                unchecked((sbyte)0x90)
            };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.IsType<BigInteger>(val.Value);
            Assert.Equal(BigInteger.Parse("12345678901234567890"), (BigInteger) val.Value);
        }

        [Fact]
        public void ParseBinary_NineteenDigits_ReturnsBigInteger()
        {
            // 19 digits: 10 BCD bytes (odd → leading nibble from first byte low nibble)
            // length=19, length%2=1, first nibble from buf[pos] low nibble
            // e.g. buf[0]=0x01 → first digit=1; buf[1..9] = 0x23,0x45,0x67,0x89,0x01,0x23,0x45,0x67,0x89
            // → "1234567890123456789"
            var fpi = new NumericParseInfo(19);
            var buf = new sbyte[]
            {
                0x01, 0x23, 0x45, 0x67,
                unchecked((sbyte)0x89), 0x01, 0x23, 0x45, 0x67,
                unchecked((sbyte)0x89)
            };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.IsType<BigInteger>(val.Value);
            Assert.Equal(BigInteger.Parse("1234567890123456789"), (BigInteger) val.Value);
        }

        [Fact]
        public void ParseBinary_BigInteger_NegativePosition_Throws()
        {
            var fpi = new NumericParseInfo(20);
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[10], -1, null));
        }

        [Fact]
        public void ParseBinary_BigInteger_InsufficientData_Throws()
        {
            // length=20 needs 10 bytes; buf only has 5
            var fpi = new NumericParseInfo(20);
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[5], 0, null));
        }
    }
}
