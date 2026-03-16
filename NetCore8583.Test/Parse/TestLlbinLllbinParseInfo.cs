using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Tests for LlbinParseInfo (LLBIN) and LllbinParseInfo (LLLBIN).
    ///
    /// ASCII format:
    ///   LLBIN  : 2-digit decimal count of HEX chars  + hex data
    ///   LLLBIN : 3-digit decimal count of HEX chars  + hex data
    ///
    /// Binary format:
    ///   LLBIN  : 1 BCD byte (byte count)             + raw bytes
    ///   LLLBIN : 2 BCD nibble bytes (byte count)      + raw bytes
    /// </summary>
    public class TestLlbinLllbinParseInfo
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

        // helper: ASCII bytes from string
        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

        // ═══════════════════════════════════════════════════════════════════════
        // LlbinParseInfo – Parse (ASCII)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Llbin_Parse_DecodesHexData()
        {
            // header "04" = 4 hex chars, data "0102" = bytes {1, 2}
            var fpi = new LlbinParseInfo();
            var buf = Ascii("040102");
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Equal(IsoType.LLBIN, val.Type);
            Assert.Equal(new sbyte[] { 1, 2 }, (sbyte[]) val.Value);
        }

        [Fact]
        public void Llbin_Parse_ZeroLength_ReturnsEmpty()
        {
            var fpi = new LlbinParseInfo();
            var buf = Ascii("00");
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Empty((sbyte[]) val.Value);
        }

        [Fact]
        public void Llbin_Parse_WithOffset()
        {
            var fpi = new LlbinParseInfo();
            var buf = Ascii("XX040102");
            var val = fpi.Parse(1, buf, 2, null);
            Assert.Equal(new sbyte[] { 1, 2 }, (sbyte[]) val.Value);
        }

        [Fact]
        public void Llbin_Parse_NegativePos_Throws()
        {
            var fpi = new LlbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("040102"), -1, null));
        }

        [Fact]
        public void Llbin_Parse_ShortHeader_Throws()
        {
            var fpi = new LlbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("0"), 0, null));
        }

        [Fact]
        public void Llbin_Parse_InsufficientData_Throws()
        {
            var fpi = new LlbinParseInfo();
            // header says 8 hex chars but only 2 provided
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("0801"), 0, null));
        }

        [Fact]
        public void Llbin_Parse_CustomBinaryField_NonNull()
        {
            var decoded = new sbyte[] { 0x0a, 0x0b };
            var fpi = new LlbinParseInfo();
            var buf = Ascii("040102");
            var val = fpi.Parse(1, buf, 0, new BinaryReturnDecoded(decoded));
            Assert.Equal(decoded, val.Value);
        }

        [Fact]
        public void Llbin_Parse_CustomBinaryField_Null_FallsBack()
        {
            var fpi = new LlbinParseInfo();
            var buf = Ascii("040102");
            var val = fpi.Parse(1, buf, 0, new BinaryReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        [Fact]
        public void Llbin_Parse_CustomField_NonNull()
        {
            var fpi = new LlbinParseInfo();
            var buf = Ascii("040102");
            var val = fpi.Parse(1, buf, 0, new ReturnDecoded("custom"));
            Assert.Equal("custom", val.ToString());
        }

        [Fact]
        public void Llbin_Parse_CustomField_Null_FallsBack()
        {
            var fpi = new LlbinParseInfo();
            var buf = Ascii("040102");
            var val = fpi.Parse(1, buf, 0, new ReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // LlbinParseInfo – ParseBinary
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Llbin_ParseBinary_ReturnsRawBytes()
        {
            // 1-byte BCD header: 0x02 = 2 bytes; then raw bytes 0x01 0x02
            var fpi = new LlbinParseInfo();
            var buf = new sbyte[] { 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.LLBIN, val.Type);
            Assert.Equal(new sbyte[] { 0x01, 0x02 }, (sbyte[]) val.Value);
        }

        [Fact]
        public void Llbin_ParseBinary_ZeroLength_ReturnsEmpty()
        {
            var fpi = new LlbinParseInfo();
            var buf = new sbyte[] { 0x00 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Empty((sbyte[]) val.Value);
        }

        [Fact]
        public void Llbin_ParseBinary_NegativePos_Throws()
        {
            var fpi = new LlbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 2, 1, 2 }, -1, null));
        }

        [Fact]
        public void Llbin_ParseBinary_ShortBuffer_Throws()
        {
            var fpi = new LlbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[0], 0, null));
        }

        [Fact]
        public void Llbin_ParseBinary_InsufficientData_Throws()
        {
            var fpi = new LlbinParseInfo();
            // header says 10 bytes but only 1 available
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x10 }, 0, null));
        }

        [Fact]
        public void Llbin_ParseBinary_CustomBinaryField_NonNull()
        {
            var decoded = new sbyte[] { 0x0a, 0x0b };
            var fpi = new LlbinParseInfo();
            var buf = new sbyte[] { 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new BinaryReturnDecoded(decoded));
            Assert.Equal(decoded, val.Value);
        }

        [Fact]
        public void Llbin_ParseBinary_CustomBinaryField_Null_FallsBack()
        {
            var fpi = new LlbinParseInfo();
            var buf = new sbyte[] { 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new BinaryReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        [Fact]
        public void Llbin_ParseBinary_CustomField_NonNull()
        {
            var fpi = new LlbinParseInfo();
            var buf = new sbyte[] { 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new ReturnDecoded("custom"));
            Assert.Equal("custom", val.ToString());
        }

        [Fact]
        public void Llbin_ParseBinary_CustomField_Null_FallsBack()
        {
            var fpi = new LlbinParseInfo();
            var buf = new sbyte[] { 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new ReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // LllbinParseInfo – Parse (ASCII)
        // 3-digit header = count of hex chars; data is hex-encoded
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Lllbin_Parse_DecodesHexData()
        {
            // header "004" = 4 hex chars, data "0102" = bytes {1, 2}
            var fpi = new LllbinParseInfo();
            var buf = Ascii("0040102");
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Equal(IsoType.LLLBIN, val.Type);
            Assert.Equal(new sbyte[] { 1, 2 }, (sbyte[]) val.Value);
        }

        [Fact]
        public void Lllbin_Parse_ZeroLength_ReturnsEmpty()
        {
            var fpi = new LllbinParseInfo();
            var buf = Ascii("000");
            var val = fpi.Parse(1, buf, 0, null);
            Assert.Empty((sbyte[]) val.Value);
        }

        [Fact]
        public void Lllbin_Parse_NegativePos_Throws()
        {
            var fpi = new LllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("0040102"), -1, null));
        }

        [Fact]
        public void Lllbin_Parse_ShortHeader_Throws()
        {
            var fpi = new LllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("00"), 0, null));
        }

        [Fact]
        public void Lllbin_Parse_InsufficientData_Throws()
        {
            var fpi = new LllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("00801"), 0, null));
        }

        [Fact]
        public void Lllbin_Parse_CustomBinaryField_NonNull()
        {
            var decoded = new sbyte[] { 0x0a, 0x0b };
            var fpi = new LllbinParseInfo();
            var buf = Ascii("0040102");
            var val = fpi.Parse(1, buf, 0, new BinaryReturnDecoded(decoded));
            Assert.Equal(decoded, val.Value);
        }

        [Fact]
        public void Lllbin_Parse_CustomBinaryField_Null_FallsBack()
        {
            var fpi = new LllbinParseInfo();
            var buf = Ascii("0040102");
            var val = fpi.Parse(1, buf, 0, new BinaryReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        [Fact]
        public void Lllbin_Parse_CustomField_NonNull()
        {
            var fpi = new LllbinParseInfo();
            var buf = Ascii("0040102");
            var val = fpi.Parse(1, buf, 0, new ReturnDecoded("custom"));
            Assert.Equal("custom", val.ToString());
        }

        [Fact]
        public void Lllbin_Parse_CustomField_Null_FallsBack()
        {
            var fpi = new LllbinParseInfo();
            var buf = Ascii("0040102");
            var val = fpi.Parse(1, buf, 0, new ReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // LllbinParseInfo – ParseBinary
        // 2-byte BCD: nibbles = hundreds, tens, units of byte count
        // byte 0 low nibble * 100 + byte 1 high nibble * 10 + byte 1 low nibble
        // e.g. 0x00, 0x03 → 0*100 + 0*10 + 3 = 3 bytes
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Lllbin_ParseBinary_ReturnsRawBytes()
        {
            var fpi = new LllbinParseInfo();
            // header: 0x00 0x03 → l = (0&0x0f)*100 + ((3&0xf0)>>4)*10 + (3&0x0f) = 3
            var buf = new sbyte[] { 0x00, 0x03, 0x0a, 0x0b, 0x0c };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.LLLBIN, val.Type);
            Assert.Equal(new sbyte[] { 0x0a, 0x0b, 0x0c }, (sbyte[]) val.Value);
        }

        [Fact]
        public void Lllbin_ParseBinary_NegativePos_Throws()
        {
            var fpi = new LllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0, 2, 1, 2 }, -1, null));
        }

        [Fact]
        public void Lllbin_ParseBinary_ShortBuffer_Throws()
        {
            var fpi = new LllbinParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0 }, 0, null));
        }

        [Fact]
        public void Lllbin_ParseBinary_InsufficientData_Throws()
        {
            var fpi = new LllbinParseInfo();
            // header says 10 bytes but only header provided
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x00, 0x10 }, 0, null));
        }

        [Fact]
        public void Lllbin_ParseBinary_CustomBinaryField_NonNull()
        {
            var decoded = new sbyte[] { 0x0a, 0x0b };
            var fpi = new LllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new BinaryReturnDecoded(decoded));
            Assert.Equal(decoded, val.Value);
        }

        [Fact]
        public void Lllbin_ParseBinary_CustomBinaryField_Null_FallsBack()
        {
            var fpi = new LllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new BinaryReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }

        [Fact]
        public void Lllbin_ParseBinary_CustomField_NonNull()
        {
            var fpi = new LllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new ReturnDecoded("custom"));
            Assert.Equal("custom", val.ToString());
        }

        [Fact]
        public void Lllbin_ParseBinary_CustomField_Null_FallsBack()
        {
            var fpi = new LllbinParseInfo();
            var buf = new sbyte[] { 0x00, 0x02, 0x01, 0x02 };
            var val = fpi.ParseBinary(1, buf, 0, new ReturnNull());
            Assert.IsType<sbyte[]>(val.Value);
        }
    }
}
