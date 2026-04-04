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

using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Unit tests for <see cref="AmountParseInfo"/>.
    ///
    /// ASCII format: 12 decimal digit characters representing cents (divide by 100 to get amount).
    ///   e.g. "000000001000" → 10.00
    ///
    /// Binary format: 6 BCD bytes encoding 12 digits with an implied decimal point after digit 10.
    ///   e.g. {0x00,0x00,0x00,0x00,0x10,0x00} → "0000000010.00" → 10.00
    /// </summary>
    public class TestAmountParseInfo
    {
        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

        // ═══════════════════════════════════════════════════════════════════════
        // Parse (ASCII)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Parse_ReturnsCorrectAmount()
        {
            var fpi = new AmountParseInfo();
            var val = fpi.Parse(1, Ascii("000000001000"), 0, null);
            Assert.Equal(IsoType.AMOUNT, val.Type);
            Assert.Equal(10.00m, (decimal) val.Value);
        }

        [Fact]
        public void Parse_OneCent()
        {
            var fpi = new AmountParseInfo();
            var val = fpi.Parse(1, Ascii("000000000001"), 0, null);
            Assert.Equal(0.01m, (decimal) val.Value);
        }

        [Fact]
        public void Parse_ZeroAmount()
        {
            var fpi = new AmountParseInfo();
            var val = fpi.Parse(1, Ascii("000000000000"), 0, null);
            Assert.Equal(0.00m, (decimal) val.Value);
        }

        [Fact]
        public void Parse_LargeAmount()
        {
            var fpi = new AmountParseInfo();
            var val = fpi.Parse(1, Ascii("999999999999"), 0, null);
            Assert.Equal(9999999999.99m, (decimal) val.Value);
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new AmountParseInfo();
            // 4 bytes prefix + 12 bytes of amount
            var buf = Ascii("XXXX000000005000");
            var val = fpi.Parse(1, buf, 4, null);
            Assert.Equal(50.00m, (decimal) val.Value);
        }

        [Fact]
        public void Parse_NegativePosition_Throws()
        {
            var fpi = new AmountParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("000000001000"), -1, null));
        }

        [Fact]
        public void Parse_InsufficientData_Throws()
        {
            var fpi = new AmountParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("00000010"), 0, null));
        }

        [Fact]
        public void Parse_InsufficientDataFromOffset_Throws()
        {
            var fpi = new AmountParseInfo();
            // only 5 chars after offset 7 → not enough for 12
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("000000012345"), 7, null));
        }

        [Fact]
        public void Parse_InvalidFormat_Throws()
        {
            var fpi = new AmountParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("00000000ABCD"), 0, null));
        }

        [Fact]
        public void Parse_HasCorrectLength()
        {
            var fpi = new AmountParseInfo();
            var val = fpi.Parse(1, Ascii("000000002500"), 0, null);
            Assert.Equal(12, val.Length);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary (BCD)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_TenDollars()
        {
            // BCD for 12 digits "000000001000" → 6 bytes {0x00,0x00,0x00,0x00,0x10,0x00}
            // → "0000000010.00" → 10.00
            var fpi = new AmountParseInfo();
            var buf = new sbyte[] { 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.AMOUNT, val.Type);
            Assert.Equal(10.00m, (decimal) val.Value);
        }

        [Fact]
        public void ParseBinary_OneCent()
        {
            // BCD "000000000001" → {0x00,0x00,0x00,0x00,0x00,0x01} → "0000000000.01" → 0.01
            var fpi = new AmountParseInfo();
            var buf = new sbyte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(0.01m, (decimal) val.Value);
        }

        [Fact]
        public void ParseBinary_ZeroAmount()
        {
            var fpi = new AmountParseInfo();
            var buf = new sbyte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(0.00m, (decimal) val.Value);
        }

        [Fact]
        public void ParseBinary_WithOffset()
        {
            // 2 bytes of garbage then BCD for $25.00: {0x00,0x00,0x00,0x00,0x25,0x00}
            // → "0000000025.00" → 25.00
            var fpi = new AmountParseInfo();
            var buf = new sbyte[] { 0x11, 0x22, 0x00, 0x00, 0x00, 0x00, 0x25, 0x00 };
            var val = fpi.ParseBinary(1, buf, 2, null);
            Assert.Equal(25.00m, (decimal) val.Value);
        }

        [Fact]
        public void ParseBinary_LargeAmount()
        {
            // BCD "999999999999" → {0x99,0x99,0x99,0x99,0x99,0x99} → "9999999999.99" → 9999999999.99
            var fpi = new AmountParseInfo();
            var buf = new sbyte[] { unchecked((sbyte)0x99), unchecked((sbyte)0x99), unchecked((sbyte)0x99), unchecked((sbyte)0x99), unchecked((sbyte)0x99), unchecked((sbyte)0x99) };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(9999999999.99m, (decimal) val.Value);
        }
    }
}
