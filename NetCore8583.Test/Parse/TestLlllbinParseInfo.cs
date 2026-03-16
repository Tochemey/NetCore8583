using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Tests for LlllbinParseInfo (LLLLBIN).
    ///
    /// ASCII format : 4-digit decimal count of HEX chars + hex-encoded data
    /// Binary format: 2-byte BCD (4 nibbles = 4 digits of byte count) + raw bytes
    ///   byte count = ((buf[pos] &amp; 0xf0)>>4)*1000 + (buf[pos] &amp; 0x0f)*100
    ///              + ((buf[pos+1] &amp; 0xf0)>>4)*10  + (buf[pos+1] &amp; 0x0f)
    ///   e.g. 0x00 0x02 → 0*1000 + 0*100 + 0*10 + 2 = 2 bytes
    /// </summary>
    public class TestLlllbinParseInfo
    {
        // ── custom-field helpers ─────────────────────────────────────────────────

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

        private sealed class BinaryReturnDecoded : ICustomBinaryField
        {
            private readonly sbyte[] _decoded;
            public BinaryReturnDecoded(sbyte[] decoded) => _decoded = decoded;
            public object DecodeField(string value) => _decoded;
            public string EncodeField(object value) => HexCodec.HexEncode(_decoded, 0, _decoded.Length);
            public object DecodeBinaryField(sbyte[] bytes, int offset, int length) => _decoded;
            public sbyte[] EncodeBinaryField(object value) => _decoded;
        }

        private sealed class BinaryReturnNull : ICustomBinaryField
        {
            public object DecodeField(string value) => null;
            public string EncodeField(object value) => null;
            public object DecodeBinaryField(sbyte[] bytes, int offset, int length) => null;
            public sbyte[] EncodeBinaryField(object value) => new sbyte[0];
        }

        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

        // ═══════════════════════════════════════════════════════════════════════
        // Parse (ASCII)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Parse_DecodesHexData()
        {
            // header "0004" = 4 hex chars, data "0102" = bytes {1, 2}
            var fpi = new LlllbinParseInfo();
            var buf = Ascii("00040102");
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Equal(IsoType.LLLLBIN, val.Type);
            Assert.Equal(new sbyte[] { 1, 2 }, (sbyte[]) val.Value);
        }

        [Fact]
        public void Parse_ZeroLength_ReturnsEmpty()
        {
            var fpi = new LlllbinParseInfo();
            var buf = Ascii("0000");
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Empty((sbyte[]) val.Value);
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new LlllbinParseInfo();
            var buf = Ascii("XX00040102");
            var val = fpi.Parse(1, buf, 2, null);
            Assert.Equal(new sbyte[] { 1, 2 }, (sbyte[]) val.Value);
        }

        [Fact]
        public void Parse_NegativePos_Throws()
        {
            var fpi = new LlllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("00040102"), -1, null));
        }

        [Fact]
        public void Parse_ShortHeader_Throws()
        {
            var fpi = new LlllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("000"), 0, null));
        }

        [Fact]
        public void Parse_InsufficientData_Throws()
        {
            var fpi = new LlllbinParseInfo();
            // header says 8 hex chars but only 2 provided
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("000801"), 0, null));
        }

        [Fact]
        public void Parse_CustomBinaryField_NonNull()
        {
            var decoded = new sbyte[] { 0x0a, 0x0b };
            var fpi = new LlllbinParseInfo();
            var buf = Ascii("00040102");
            var val = fpi.Parse(1, buf, 0, new BinaryReturnDecoded(decoded));
            Assert.Equal(decoded, val.Value);
        }

        [Fact]
        public void Parse_CustomBinaryField_Null_FallsBack()
        {
            var fpi = new LlllbinParseInfo();
            var buf = Ascii("00040102");
            var val = fpi.Parse(1, buf, 0, new BinaryReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        [Fact]
        public void Parse_CustomField_NonNull()
        {
            var fpi = new LlllbinParseInfo();
            var buf = Ascii("00040102");
            var val = fpi.Parse(1, buf, 0, new ReturnDecoded("custom"));
            Assert.Equal("custom", val.ToString());
        }

        [Fact]
        public void Parse_CustomField_Null_FallsBack()
        {
            var fpi = new LlllbinParseInfo();
            var buf = Ascii("00040102");
            var val = fpi.Parse(1, buf, 0, new ReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_ReturnsRawBytes()
        {
            var fpi = new LlllbinParseInfo();
            // header 0x00 0x02 → l = 0*1000+0*100+0*10+2 = 2 bytes
            var buf = new sbyte[] { 0x00, 0x02, 0x0a, 0x0b };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.LLLLBIN, val.Type);
            Assert.Equal(new sbyte[] { 0x0a, 0x0b }, (sbyte[]) val.Value);
        }

        [Fact]
        public void ParseBinary_ZeroLength_ReturnsEmpty()
        {
            var fpi = new LlllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x00 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Empty((sbyte[]) val.Value);
        }

        [Fact]
        public void ParseBinary_LargerLength()
        {
            // header 0x00 0x13 → l = 0*1000+0*100+1*10+3 = 13 bytes
            var fpi = new LlllbinParseInfo();
            var data = new sbyte[13];
            for (var i = 0; i < 13; i++) data[i] = (sbyte) (i + 1);
            var buf = new sbyte[15];
            buf[0] = 0x00;
            buf[1] = 0x13;
            data.CopyTo(buf, 2);
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(13, ((sbyte[]) val.Value).Length);
        }

        [Fact]
        public void ParseBinary_NegativePos_Throws()
        {
            var fpi = new LlllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0, 2, 1, 2 }, -1, null));
        }

        [Fact]
        public void ParseBinary_ShortBuffer_Throws()
        {
            var fpi = new LlllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0 }, 0, null));
        }

        [Fact]
        public void ParseBinary_InsufficientData_Throws()
        {
            var fpi = new LlllbinParseInfo();
            // header says 10 bytes but only header provided
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x00, 0x10 }, 0, null));
        }

        [Fact]
        public void ParseBinary_CustomBinaryField_NonNull()
        {
            var decoded = new sbyte[] { 0x0a, 0x0b };
            var fpi = new LlllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new BinaryReturnDecoded(decoded));
            Assert.Equal(decoded, val.Value);
        }

        [Fact]
        public void ParseBinary_CustomBinaryField_Null_FallsBack()
        {
            var fpi = new LlllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new BinaryReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        [Fact]
        public void ParseBinary_CustomField_NonNull()
        {
            var fpi = new LlllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new ReturnDecoded("custom"));
            Assert.Equal("custom", val.ToString());
        }

        [Fact]
        public void ParseBinary_CustomField_Null_FallsBack()
        {
            var fpi = new LlllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new ReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }
    }
}
