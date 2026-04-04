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
    /// Unit tests for <see cref="LlllvarParseInfo"/>.
    ///
    /// ASCII format: 4-digit decimal length header + string data.
    ///   e.g. "0005HELLO" → length=5, value="HELLO"
    ///
    /// Binary format: 2 BCD bytes (4 nibbles = 4-digit length) + string data.
    ///   length = ((buf[pos] &amp; 0xf0) >> 4) * 1000 + (buf[pos] &amp; 0x0f) * 100
    ///          + ((buf[pos+1] &amp; 0xf0) >> 4) * 10 + (buf[pos+1] &amp; 0x0f)
    ///   e.g. {0x00, 0x05} → 0*1000+0*100+0*10+5 = 5
    /// </summary>
    public class TestLlllvarParseInfo
    {
        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

        private static sbyte[] Concat(sbyte[] first, sbyte[] second)
        {
            var result = new sbyte[first.Length + second.Length];
            first.CopyTo(result, 0);
            second.CopyTo(result, first.Length);
            return result;
        }

        private sealed class ReturnDecoded : ICustomField
        {
            private readonly object _val;
            public ReturnDecoded(object val) => _val = val;
            public object DecodeField(string value) => _val;
            public string EncodeField(object value) => value?.ToString();
        }

        private sealed class ReturnNull : ICustomField
        {
            public object DecodeField(string value) => null;
            public string EncodeField(object value) => value?.ToString();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Parse (ASCII)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Parse_ReturnsCorrectValue()
        {
            var fpi = new LlllvarParseInfo();
            var val = fpi.Parse(1, Ascii("0005HELLO"), 0, null);
            Assert.Equal(IsoType.LLLLVAR, val.Type);
            Assert.Equal("HELLO", val.Value);
            Assert.Equal(5, val.Length);
        }

        [Fact]
        public void Parse_ZeroLength_ReturnsEmptyString()
        {
            var fpi = new LlllvarParseInfo();
            var val = fpi.Parse(1, Ascii("0000"), 0, null);
            Assert.Equal(string.Empty, val.Value);
            Assert.Equal(0, val.Length);
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new LlllvarParseInfo();
            var buf = Ascii("XXXX0003ABC");
            var val = fpi.Parse(1, buf, 4, null);
            Assert.Equal("ABC", val.Value);
            Assert.Equal(3, val.Length);
        }

        [Fact]
        public void Parse_LongerData()
        {
            var fpi = new LlllvarParseInfo();
            var data = new string('Z', 500);
            var buf = Ascii("0500" + data);
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Equal(data, val.Value);
            Assert.Equal(500, val.Length);
        }

        [Fact]
        public void Parse_NegativePosition_Throws()
        {
            var fpi = new LlllvarParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("0005HELLO"), -1, null));
        }

        [Fact]
        public void Parse_InsufficientHeaderData_Throws()
        {
            var fpi = new LlllvarParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("000"), 0, null));
        }

        [Fact]
        public void Parse_InsufficientFieldData_Throws()
        {
            var fpi = new LlllvarParseInfo();
            // Header says 10 but only 3 chars follow
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("0010ABC"), 0, null));
        }

        [Fact]
        public void Parse_CustomField_NonNullDecoded()
        {
            var fpi = new LlllvarParseInfo();
            var val = fpi.Parse(1, Ascii("0005HELLO"), 0, new ReturnDecoded("WORLD"));
            Assert.Equal("WORLD", val.Value);
        }

        [Fact]
        public void Parse_CustomField_NullDecoded_FallsBackToString()
        {
            var fpi = new LlllvarParseInfo();
            var val = fpi.Parse(1, Ascii("0005HELLO"), 0, new ReturnNull());
            Assert.Equal("HELLO", val.Value);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary
        // 2 bytes for 4-nibble length
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_ReturnsCorrectValue()
        {
            // {0x00, 0x05}: 0*1000+0*100+0*10+5 = 5
            var fpi = new LlllvarParseInfo();
            var buf = Concat(new sbyte[] { 0x00, 0x05 }, Ascii("HELLO"));
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.LLLLVAR, val.Type);
            Assert.Equal("HELLO", val.Value);
        }

        [Fact]
        public void ParseBinary_ZeroLength_ReturnsEmptyString()
        {
            var fpi = new LlllvarParseInfo();
            var val = fpi.ParseBinary(1, new sbyte[] { 0x00, 0x00 }, 0, null);
            Assert.Equal(string.Empty, val.Value);
        }

        [Fact]
        public void ParseBinary_LengthOverThousand()
        {
            // {0x10, 0x00}: 1*1000+0*100+0*10+0 = 1000
            var fpi = new LlllvarParseInfo();
            var data = Ascii(new string('A', 1000));
            var buf = Concat(new sbyte[] { 0x10, 0x00 }, data);
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(1000, ((string) val.Value).Length);
        }

        [Fact]
        public void ParseBinary_LengthDecoding()
        {
            // {0x01, 0x23}: 0*1000+1*100+2*10+3 = 123
            var fpi = new LlllvarParseInfo();
            var data = Ascii(new string('B', 123));
            var buf = Concat(new sbyte[] { 0x01, 0x23 }, data);
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(123, ((string) val.Value).Length);
        }

        [Fact]
        public void ParseBinary_WithOffset()
        {
            var fpi = new LlllvarParseInfo();
            var prefix = new sbyte[] { 0x00, 0x00 };
            var header = new sbyte[] { 0x00, 0x03 };
            var data = Ascii("ABC");
            var buf = Concat(Concat(prefix, header), data);
            var val = fpi.ParseBinary(1, buf, 2, null);
            Assert.Equal("ABC", val.Value);
        }

        [Fact]
        public void ParseBinary_NegativePosition_Throws()
        {
            var fpi = new LlllvarParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x00, 0x05, 72, 69, 76, 76, 79 }, -1, null));
        }

        [Fact]
        public void ParseBinary_ShortBuffer_Throws()
        {
            var fpi = new LlllvarParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x00 }, 0, null));
        }

        [Fact]
        public void ParseBinary_InsufficientFieldData_Throws()
        {
            var fpi = new LlllvarParseInfo();
            // header says 10 bytes but only the 2-byte header is present
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x00, 0x10 }, 0, null));
        }

        [Fact]
        public void ParseBinary_CustomField_NonNullDecoded()
        {
            var fpi = new LlllvarParseInfo();
            var buf = Concat(new sbyte[] { 0x00, 0x05 }, Ascii("HELLO"));
            var val = fpi.ParseBinary(1, buf, 0, new ReturnDecoded("WORLD"));
            Assert.Equal("WORLD", val.Value);
        }

        [Fact]
        public void ParseBinary_CustomField_NullDecoded_FallsBackToString()
        {
            var fpi = new LlllvarParseInfo();
            var buf = Concat(new sbyte[] { 0x00, 0x05 }, Ascii("HELLO"));
            var val = fpi.ParseBinary(1, buf, 0, new ReturnNull());
            Assert.Equal("HELLO", val.Value);
        }
    }
}
