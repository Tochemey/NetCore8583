using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class TestBinaryParseInfo
    {
        // Helpers ----------------------------------------------------------------

        private static sbyte[] HexBuf(string hex) => HexCodec.HexDecode(hex);

        // Returns a sbyte[] decoded value; EncodeField re-encodes as hex so length is consistent.
        private sealed class BinaryDecoded : ICustomField
        {
            private readonly sbyte[] _decoded;
            public BinaryDecoded(sbyte[] decoded) => _decoded = decoded;
            public object DecodeField(string value) => _decoded;
            public string EncodeField(object value) => HexCodec.HexEncode((sbyte[]) value, 0, ((sbyte[]) value).Length);
        }

        private sealed class ReturnNull : ICustomField
        {
            public object DecodeField(string value) => null;
            public string EncodeField(object value) => value?.ToString();
        }

        // ── Parse (ASCII hex-encoded) ────────────────────────────────────────────

        [Fact]
        public void Parse_DecodesHexEncodedBytes()
        {
            // 4-byte BINARY: "01020304" in ASCII = bytes 0x01 0x02 0x03 0x04
            var fpi = new BinaryParseInfo(4);
            var buf = "01020304".GetSignedBytes(Encoding.ASCII);
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Equal(IsoType.BINARY, val.Type);
            var bytes = (sbyte[]) val.Value;
            Assert.Equal(new sbyte[] { 1, 2, 3, 4 }, bytes);
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new BinaryParseInfo(2);
            var buf = "deadbeef".GetSignedBytes(Encoding.ASCII);
            // offset 4 → "beef" → 0xbe 0xef
            var val = fpi.Parse(1, buf, 4, null);
            var bytes = (sbyte[]) val.Value;
            Assert.Equal(unchecked((sbyte) 0xbe), bytes[0]);
            Assert.Equal(unchecked((sbyte) 0xef), bytes[1]);
        }

        [Fact]
        public void Parse_CustomField_NonNullDecoded()
        {
            var decoded = new sbyte[] { 0x0a, 0x0b };
            var fpi = new BinaryParseInfo(2);
            var buf = "0102".GetSignedBytes(Encoding.ASCII);
            var val = fpi.Parse(1, buf, 0, new BinaryDecoded(decoded));
            Assert.Equal(decoded, val.Value);
        }

        [Fact]
        public void Parse_CustomField_NullDecoded_FallsBackToBytes()
        {
            var fpi = new BinaryParseInfo(2);
            var buf = "0102".GetSignedBytes(Encoding.ASCII);
            var val = fpi.Parse(1, buf, 0, new ReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        [Fact]
        public void Parse_NegativePosition_Throws()
        {
            var fpi = new BinaryParseInfo(2);
            Assert.Throws<ParseException>(() => fpi.Parse(1, "0102".GetSignedBytes(Encoding.ASCII), -1, null));
        }

        [Fact]
        public void Parse_InsufficientData_Throws()
        {
            var fpi = new BinaryParseInfo(8);
            Assert.Throws<ParseException>(() => fpi.Parse(1, "0102".GetSignedBytes(Encoding.ASCII), 0, null));
        }

        // ── ParseBinary (raw bytes) ──────────────────────────────────────────────

        [Fact]
        public void ParseBinary_ReturnsRawBytes()
        {
            var fpi = new BinaryParseInfo(3);
            var buf = new sbyte[] { 0x0a, 0x0b, 0x0c };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.BINARY, val.Type);
            Assert.Equal(buf, (sbyte[]) val.Value);
        }

        [Fact]
        public void ParseBinary_WithOffset()
        {
            var fpi = new BinaryParseInfo(2);
            var buf = new sbyte[] { 0x00, 0x00, 0x0a, 0x0b };
            var val = fpi.ParseBinary(1, buf, 2, null);
            Assert.Equal(new sbyte[] { 0x0a, 0x0b }, (sbyte[]) val.Value);
        }

        [Fact]
        public void ParseBinary_CustomField_NonNullDecoded()
        {
            var decoded = new sbyte[] { 0x0a, 0x0b };
            var fpi = new BinaryParseInfo(2);
            var buf = new sbyte[] { 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new BinaryDecoded(decoded));
            Assert.Equal(decoded, val.Value);
        }

        [Fact]
        public void ParseBinary_CustomField_NullDecoded_FallsBackToBytes()
        {
            var fpi = new BinaryParseInfo(2);
            var buf = new sbyte[] { 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new ReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        [Fact]
        public void ParseBinary_NegativePosition_Throws()
        {
            var fpi = new BinaryParseInfo(2);
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x01, 0x02 }, -1, null));
        }

        [Fact]
        public void ParseBinary_InsufficientData_Throws()
        {
            var fpi = new BinaryParseInfo(8);
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x01, 0x02 }, 0, null));
        }
    }
}
