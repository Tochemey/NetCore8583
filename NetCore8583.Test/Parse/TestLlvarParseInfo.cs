using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Unit tests for <see cref="LlvarParseInfo"/>.
    ///
    /// ASCII format: 2-digit decimal length header + string data.
    ///   e.g. "05HELLO" → length=5, value="HELLO"
    ///
    /// Binary format: 1 BCD byte (2-digit length) + string data.
    ///   length = ((buf[pos] &amp; 0xf0) >> 4) * 10 + (buf[pos] &amp; 0x0f)
    ///   e.g. 0x05 → length=5
    /// </summary>
    public class TestLlvarParseInfo
    {
        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

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
            var fpi = new LlvarParseInfo();
            var val = fpi.Parse(1, Ascii("05HELLO"), 0, null);
            Assert.Equal(IsoType.LLVAR, val.Type);
            Assert.Equal("HELLO", val.Value);
            Assert.Equal(5, val.Length);
        }

        [Fact]
        public void Parse_ZeroLength_ReturnsEmptyString()
        {
            var fpi = new LlvarParseInfo();
            var val = fpi.Parse(1, Ascii("00"), 0, null);
            Assert.Equal(string.Empty, val.Value);
            Assert.Equal(0, val.Length);
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new LlvarParseInfo();
            var buf = Ascii("XXXX03ABC");
            var val = fpi.Parse(1, buf, 4, null);
            Assert.Equal("ABC", val.Value);
            Assert.Equal(3, val.Length);
        }

        [Fact]
        public void Parse_MaxTwoDigitLength()
        {
            // length=99: "99" + 99 'A' characters
            var fpi = new LlvarParseInfo();
            var data = new string('A', 99);
            var buf = Ascii("99" + data);
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Equal(data, val.Value);
            Assert.Equal(99, val.Length);
        }

        [Fact]
        public void Parse_NegativePosition_Throws()
        {
            var fpi = new LlvarParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("05HELLO"), -1, null));
        }

        [Fact]
        public void Parse_InsufficientHeaderData_Throws()
        {
            var fpi = new LlvarParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("0"), 0, null));
        }

        [Fact]
        public void Parse_InsufficientFieldData_Throws()
        {
            var fpi = new LlvarParseInfo();
            // Header says length=10 but only 3 chars follow
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("10ABC"), 0, null));
        }

        [Fact]
        public void Parse_CustomField_NonNullDecoded()
        {
            var fpi = new LlvarParseInfo();
            var val = fpi.Parse(1, Ascii("05HELLO"), 0, new ReturnDecoded("WORLD"));
            Assert.Equal("WORLD", val.Value);
        }

        [Fact]
        public void Parse_CustomField_NullDecoded_FallsBackToString()
        {
            var fpi = new LlvarParseInfo();
            var val = fpi.Parse(1, Ascii("05HELLO"), 0, new ReturnNull());
            Assert.Equal("HELLO", val.Value);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_ReturnsCorrectValue()
        {
            // BCD header 0x05 → length=5, then "HELLO"
            var fpi = new LlvarParseInfo();
            var buf = new sbyte[] { 0x05 }.Concat(Ascii("HELLO"));
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.LLVAR, val.Type);
            Assert.Equal("HELLO", val.Value);
        }

        [Fact]
        public void ParseBinary_ZeroLength_ReturnsEmptyString()
        {
            var fpi = new LlvarParseInfo();
            var val = fpi.ParseBinary(1, new sbyte[] { 0x00 }, 0, null);
            Assert.Equal(string.Empty, val.Value);
        }

        [Fact]
        public void ParseBinary_LengthDecodedFromBcd()
        {
            // 0x13 = (1<<4)|3 → 1*10+3 = 13
            var fpi = new LlvarParseInfo();
            var data = Ascii("ABCDEFGHIJKLM"); // 13 chars
            var buf = new sbyte[] { 0x13 }.Concat(data);
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal("ABCDEFGHIJKLM", val.Value);
        }

        [Fact]
        public void ParseBinary_WithOffset()
        {
            var fpi = new LlvarParseInfo();
            var prefix = new sbyte[] { 0x00, 0x00 };
            var header = new sbyte[] { 0x03 };
            var data = Ascii("ABC");
            var buf = prefix.Concat(header).Concat(data);
            var val = fpi.ParseBinary(1, buf, 2, null);
            Assert.Equal("ABC", val.Value);
        }

        [Fact]
        public void ParseBinary_NegativePosition_Throws()
        {
            var fpi = new LlvarParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x05, 72, 69, 76, 76, 79 }, -1, null));
        }

        [Fact]
        public void ParseBinary_ShortBuffer_Throws()
        {
            var fpi = new LlvarParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[0], 0, null));
        }

        [Fact]
        public void ParseBinary_InsufficientFieldData_Throws()
        {
            var fpi = new LlvarParseInfo();
            // header says 10 bytes, but only header present
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x10 }, 0, null));
        }

        [Fact]
        public void ParseBinary_CustomField_NonNullDecoded()
        {
            var fpi = new LlvarParseInfo();
            var buf = new sbyte[] { 0x05 }.Concat(Ascii("HELLO"));
            var val = fpi.ParseBinary(1, buf, 0, new ReturnDecoded("WORLD"));
            Assert.Equal("WORLD", val.Value);
        }

        [Fact]
        public void ParseBinary_CustomField_NullDecoded_FallsBackToString()
        {
            var fpi = new LlvarParseInfo();
            var buf = new sbyte[] { 0x05 }.Concat(Ascii("HELLO"));
            var val = fpi.ParseBinary(1, buf, 0, new ReturnNull());
            Assert.Equal("HELLO", val.Value);
        }
    }

    internal static class SbyteArrayExtensions
    {
        internal static sbyte[] Concat(this sbyte[] first, sbyte[] second)
        {
            var result = new sbyte[first.Length + second.Length];
            first.CopyTo(result, 0);
            second.CopyTo(result, first.Length);
            return result;
        }
    }
}
