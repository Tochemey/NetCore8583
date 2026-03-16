using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class TestAlphaParseInfo
    {
        // ── helpers ─────────────────────────────────────────────────────────────

        private static sbyte[] Buf(string s) => s.GetSignedBytes(Encoding.ASCII);

        private sealed class ReturnDecoded : ICustomField
        {
            private readonly object _decoded;
            public ReturnDecoded(object decoded) => _decoded = decoded;
            public object DecodeField(string value) => _decoded;
            public string EncodeField(object value) => value?.ToString();
        }

        private sealed class ReturnNull : ICustomField
        {
            public object DecodeField(string value) => null;
            public string EncodeField(object value) => value?.ToString();
        }

        // ── Parse (ASCII path, inherited from AlphaNumericFieldParseInfo) ───────

        [Fact]
        public void Parse_ReturnsCorrectValue()
        {
            var fpi = new AlphaParseInfo(5);
            var val = fpi.Parse(1, Buf("HELLO"), 0, null);
            Assert.Equal("HELLO", val.ToString());
            Assert.Equal(IsoType.ALPHA, val.Type);
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new AlphaParseInfo(3);
            var val = fpi.Parse(1, Buf("XYZABC"), 3, null);
            Assert.Equal("ABC", val.ToString());
        }

        [Fact]
        public void Parse_CustomField_NonNullDecoded()
        {
            var fpi = new AlphaParseInfo(5);
            var val = fpi.Parse(1, Buf("HELLO"), 0, new ReturnDecoded("WORLD"));
            Assert.Equal("WORLD", val.ToString());
        }

        [Fact]
        public void Parse_CustomField_NullDecoded_FallsBackToRaw()
        {
            var fpi = new AlphaParseInfo(5);
            var val = fpi.Parse(1, Buf("HELLO"), 0, new ReturnNull());
            Assert.Equal("HELLO", val.ToString());
        }

        [Fact]
        public void Parse_NegativePosition_Throws()
        {
            var fpi = new AlphaParseInfo(3);
            Assert.Throws<ParseException>(() => fpi.Parse(1, Buf("ABC"), -1, null));
        }

        [Fact]
        public void Parse_InsufficientData_Throws()
        {
            var fpi = new AlphaParseInfo(10);
            Assert.Throws<ParseException>(() => fpi.Parse(1, Buf("ABC"), 0, null));
        }

        // ── ParseBinary ──────────────────────────────────────────────────────────

        [Fact]
        public void ParseBinary_ReturnsCorrectValue()
        {
            var fpi = new AlphaParseInfo(5);
            var val = fpi.ParseBinary(1, Buf("HELLO"), 0, null);
            Assert.Equal("HELLO", val.ToString());
            Assert.Equal(IsoType.ALPHA, val.Type);
        }

        [Fact]
        public void ParseBinary_WithOffset()
        {
            var fpi = new AlphaParseInfo(3);
            var val = fpi.ParseBinary(1, Buf("XYZABC"), 3, null);
            Assert.Equal("ABC", val.ToString());
        }

        [Fact]
        public void ParseBinary_CustomField_NonNullDecoded()
        {
            var fpi = new AlphaParseInfo(5);
            var val = fpi.ParseBinary(1, Buf("HELLO"), 0, new ReturnDecoded("WORLD"));
            Assert.Equal("WORLD", val.ToString());
        }

        [Fact]
        public void ParseBinary_CustomField_NullDecoded_FallsBackToRaw()
        {
            var fpi = new AlphaParseInfo(5);
            var val = fpi.ParseBinary(1, Buf("HELLO"), 0, new ReturnNull());
            Assert.Equal("HELLO", val.ToString());
        }

        [Fact]
        public void ParseBinary_NegativePosition_Throws()
        {
            var fpi = new AlphaParseInfo(3);
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, Buf("ABC"), -1, null));
        }

        [Fact]
        public void ParseBinary_InsufficientData_Throws()
        {
            var fpi = new AlphaParseInfo(10);
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, Buf("ABC"), 0, null));
        }
    }
}
