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
using System.IO;
using System.Numerics;
using System.Text;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test
{
    public class TestIsoValue
    {
        // ── Mock encoders ────────────────────────────────────────────────────────

        private sealed class StringEncoder : ICustomField
        {
            private readonly string _encoded;
            public StringEncoder(string encoded) => _encoded = encoded;
            public object DecodeField(string value) => value;
            public string EncodeField(object value) => _encoded;
        }

        private sealed class BinaryEncoder : ICustomBinaryField
        {
            private readonly sbyte[] _encoded;
            public BinaryEncoder(sbyte[] encoded) => _encoded = encoded;
            public object DecodeField(string value) => value;
            public string EncodeField(object value) => HexCodec.HexEncode(_encoded, 0, _encoded.Length);
            public object DecodeBinaryField(sbyte[] bytes, int offset, int length) => bytes;
            public sbyte[] EncodeBinaryField(object value) => _encoded;
        }

        // ── Constructor (variable-length, no explicit length) ─────────────────────

        [Fact]
        public void Constructor_NeedsLengthType_WithoutLength_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.ALPHA, "test"));
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.NUMERIC, "123"));
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.BINARY, new sbyte[2]));
        }

        [Fact]
        public void Constructor_LLVAR_WithEncoder_ComputesLengthFromEncoder()
        {
            var enc = new StringEncoder("encoded");
            var v = new IsoValue(IsoType.LLVAR, "test", enc);
            Assert.Equal(7, v.Length); // "encoded".Length
        }

        [Fact]
        public void Constructor_LLVAR_NullEncoder_ComputesLengthFromValue()
        {
            var v = new IsoValue(IsoType.LLVAR, "hello");
            Assert.Equal(5, v.Length);
        }

        [Fact]
        public void Constructor_LLVAR_TooLong_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.LLVAR, new string('a', 100)));
        }

        [Fact]
        public void Constructor_LLLVAR_TooLong_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.LLLVAR, new string('a', 1000)));
        }

        [Fact]
        public void Constructor_LLLLVAR_TooLong_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.LLLLVAR, new string('a', 10000)));
        }

        [Fact]
        public void Constructor_LLBIN_WithSbyteArray_ComputesLength()
        {
            var bytes = new sbyte[] { 1, 2, 3 };
            var v = new IsoValue(IsoType.LLBIN, bytes);
            Assert.Equal(3, v.Length);
        }

        [Fact]
        public void Constructor_LLBIN_WithStringValue_ComputesHalfLength()
        {
            // "0102" = 4 hex chars → 4/2 = 2 bytes
            var v = new IsoValue(IsoType.LLBIN, "0102");
            Assert.Equal(2, v.Length);
        }

        [Fact]
        public void Constructor_LLBIN_WithOddStringLength_RoundsUp()
        {
            // "010" = 3 chars → 3/2 + 3%2 = 2 bytes
            var v = new IsoValue(IsoType.LLBIN, "010");
            Assert.Equal(2, v.Length);
        }

        [Fact]
        public void Constructor_LLBIN_WithBinaryEncoder_ComputesLength()
        {
            var enc = new BinaryEncoder(new sbyte[] { 0x01, 0x02 });
            var v = new IsoValue(IsoType.LLBIN, "test", enc);
            Assert.Equal(2, v.Length);
        }

        [Fact]
        public void Constructor_LLBIN_TooLong_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.LLBIN, new sbyte[100]));
        }

        [Fact]
        public void Constructor_LLLBIN_TooLong_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.LLLBIN, new sbyte[1000]));
        }

        [Fact]
        public void Constructor_LLLLBIN_TooLong_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.LLLLBIN, new sbyte[10000]));
        }

        [Fact]
        public void Constructor_DateType_SetsLengthFromType()
        {
            var v = new IsoValue(IsoType.DATE10, "0125103045");
            Assert.Equal(10, v.Length);
        }

        // ── Constructor (explicit length) ─────────────────────────────────────────

        [Fact]
        public void Constructor_ExplicitLen_ZeroForNeedsLength_Throws()
        {
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.ALPHA, "test", 0));
            Assert.Throws<ArgumentException>(() => new IsoValue(IsoType.NUMERIC, "123", 0));
        }

        [Fact]
        public void Constructor_ExplicitLen_LLVAR_UsesLength()
        {
            var v = new IsoValue(IsoType.LLVAR, "hello", 5);
            Assert.Equal(5, v.Length);
        }

        [Fact]
        public void Constructor_ExplicitLen_LLBIN_UsesLength()
        {
            var v = new IsoValue(IsoType.LLBIN, new sbyte[] { 1, 2, 3 }, 3);
            Assert.Equal(3, v.Length);
        }

        // ── ToString ─────────────────────────────────────────────────────────────

        [Fact]
        public void ToString_NullValue_ReturnsIsoValueNull()
        {
            // Use explicit-length constructor to avoid NullReferenceException on null value
            var v = new IsoValue(IsoType.ALPHA, null, 3);
            Assert.Equal("ISOValue<null>", v.ToString());
        }

        [Fact]
        public void ToString_Amount_DecimalValue()
        {
            var v = new IsoValue(IsoType.AMOUNT, 12345.67m);
            Assert.Equal("000001234567", v.ToString());
        }

        [Fact]
        public void ToString_Amount_NonDecimalValue()
        {
            var v = new IsoValue(IsoType.AMOUNT, 12345);
            Assert.Equal("000001234500", v.ToString());
        }

        [Fact]
        public void ToString_Numeric_BigInteger()
        {
            var v = new IsoValue(IsoType.NUMERIC, new BigInteger(123), 6);
            Assert.Equal("000123", v.ToString());
        }

        [Fact]
        public void ToString_Numeric_Long()
        {
            var v = new IsoValue(IsoType.NUMERIC, 456L, 6);
            Assert.Equal("000456", v.ToString());
        }

        [Fact]
        public void ToString_Numeric_WithEncoder()
        {
            var enc = new StringEncoder("000999");
            var v = new IsoValue(IsoType.NUMERIC, "999", 6, enc);
            Assert.Equal("000999", v.ToString());
        }

        [Fact]
        public void ToString_Alpha_PadsRight()
        {
            var v = new IsoValue(IsoType.ALPHA, "hi", 5);
            Assert.Equal("hi   ", v.ToString());
        }

        [Fact]
        public void ToString_LlVar_NoEncoder()
        {
            var v = new IsoValue(IsoType.LLVAR, "hello");
            Assert.Equal("hello", v.ToString());
        }

        [Fact]
        public void ToString_LlVar_WithEncoder()
        {
            var enc = new StringEncoder("ENCODED");
            var v = new IsoValue(IsoType.LLVAR, "hello", enc);
            Assert.Equal("ENCODED", v.ToString());
        }

        [Fact]
        public void ToString_LllVar_NoEncoder()
        {
            var v = new IsoValue(IsoType.LLLVAR, "world");
            Assert.Equal("world", v.ToString());
        }

        [Fact]
        public void ToString_LlllVar_NoEncoder()
        {
            var v = new IsoValue(IsoType.LLLLVAR, "data");
            Assert.Equal("data", v.ToString());
        }

        [Fact]
        public void ToString_Binary_SbyteArray()
        {
            var bytes = new sbyte[] { 0x01, 0x02 };
            var v = new IsoValue(IsoType.BINARY, bytes, 2);
            Assert.Equal("0102", v.ToString());
        }

        [Fact]
        public void ToString_Binary_StringValue()
        {
            // BINARY Length=2 → Length*2=4 → Format("ABCD", 4) = "ABCD"
            var v = new IsoValue(IsoType.BINARY, "ABCD", 2);
            Assert.Equal("ABCD", v.ToString());
        }

        [Fact]
        public void ToString_LlBin_SbyteArray()
        {
            var bytes = new sbyte[] { 0x0a, 0x0b };
            var v = new IsoValue(IsoType.LLBIN, bytes);
            Assert.Equal("0A0B", v.ToString()); // HexEncode returns uppercase
        }

        [Fact]
        public void ToString_LlBin_StringValue_EvenLength()
        {
            var v = new IsoValue(IsoType.LLBIN, "abcd");
            Assert.Equal("abcd", v.ToString());
        }

        [Fact]
        public void ToString_LlBin_StringValue_OddLength_PrependZero()
        {
            var v = new IsoValue(IsoType.LLBIN, "abc");
            Assert.Equal("0abc", v.ToString());
        }

        [Fact]
        public void ToString_DateTime_FormatsCorrectly()
        {
            var dt = new DateTime(2023, 3, 15, 12, 30, 45, DateTimeKind.Utc);
            var v = new IsoValue(IsoType.DATE10, dt);
            Assert.Equal("0315123045", v.ToString());
        }

        [Fact]
        public void ToString_Date4()
        {
            var dt = new DateTime(2023, 3, 15, 12, 0, 0, DateTimeKind.Utc);
            var v = new IsoValue(IsoType.DATE4, dt);
            Assert.Equal("0315", v.ToString());
        }

        // ── Clone ─────────────────────────────────────────────────────────────────

        [Fact]
        public void Clone_ReturnsSameTypeAndValue()
        {
            var v = new IsoValue(IsoType.ALPHA, "hello", 5);
            var clone = (IsoValue) v.Clone();
            Assert.NotSame(v, clone);
            Assert.Equal(v.Type, clone.Type);
            Assert.Equal(v.Value, clone.Value);
            Assert.Equal(v.Length, clone.Length);
        }

        // ── Equals / GetHashCode ──────────────────────────────────────────────────

        [Fact]
        public void Equals_SameTypeValueLength_ReturnsTrue()
        {
            var a = new IsoValue(IsoType.ALPHA, "hello", 5);
            var b = new IsoValue(IsoType.ALPHA, "hello", 5);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            var a = new IsoValue(IsoType.ALPHA, "hello", 5);
            var b = new IsoValue(IsoType.ALPHA, "world", 5);
            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equals_NonIsoValue_ReturnsFalse()
        {
            var a = new IsoValue(IsoType.ALPHA, "hello", 5);
            Assert.False(a.Equals("hello"));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_SameValueSameHash()
        {
            var a = new IsoValue(IsoType.ALPHA, "hello", 5);
            var b = new IsoValue(IsoType.ALPHA, "hello", 5);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void GetHashCode_NullValue_ReturnsZero()
        {
            // Use explicit-length constructor to avoid NullReferenceException on null value
            var v = new IsoValue(IsoType.ALPHA, null, 3);
            Assert.Equal(0, v.GetHashCode());
        }

        // ── Write (ASCII / binary) ────────────────────────────────────────────────

        private static byte[] WriteToBytes(IsoValue v, bool binary, bool forceStringEncoding)
        {
            var stream = new MemoryStream();
            v.Encoding = Encoding.ASCII;
            v.Write(stream, binary, forceStringEncoding);
            return stream.ToArray();
        }

        [Fact]
        public void Write_LLVAR_Ascii_WritesLengthThenValue()
        {
            var v = new IsoValue(IsoType.LLVAR, "hello");
            var bytes = WriteToBytes(v, false, false);
            // "05hello" in ASCII
            Assert.Equal(new byte[] { 0x30, 0x35, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o' }, bytes);
        }

        [Fact]
        public void Write_LLVAR_Binary_WritesBcdLengthThenAsciiValue()
        {
            var v = new IsoValue(IsoType.LLVAR, "abc");
            var bytes = WriteToBytes(v, true, false);
            // BCD length byte = 0x03, then 'a','b','c'
            Assert.Equal(new byte[] { 0x03, (byte)'a', (byte)'b', (byte)'c' }, bytes);
        }

        [Fact]
        public void Write_LLLVAR_Ascii_WritesThreeDigitHeader()
        {
            var v = new IsoValue(IsoType.LLLVAR, "abc");
            var bytes = WriteToBytes(v, false, false);
            // "003abc"
            Assert.Equal(new byte[] { 0x30, 0x30, 0x33, (byte)'a', (byte)'b', (byte)'c' }, bytes);
        }

        [Fact]
        public void Write_LLLVAR_Binary_WritesTwoByteBcdHeader()
        {
            var v = new IsoValue(IsoType.LLLVAR, "abc");
            var bytes = WriteToBytes(v, true, false);
            // BCD: {0x00, 0x03}, then 'a','b','c'
            Assert.Equal(new byte[] { 0x00, 0x03, (byte)'a', (byte)'b', (byte)'c' }, bytes);
        }

        [Fact]
        public void Write_LLLLVAR_Ascii_WritesFourDigitHeader()
        {
            var v = new IsoValue(IsoType.LLLLVAR, "abc");
            var bytes = WriteToBytes(v, false, false);
            // "0003abc"
            Assert.Equal(new byte[] { 0x30, 0x30, 0x30, 0x33, (byte)'a', (byte)'b', (byte)'c' }, bytes);
        }

        [Fact]
        public void Write_LLLLVAR_Binary_WritesTwoByteBcdHeader()
        {
            var v = new IsoValue(IsoType.LLLLVAR, "abc");
            var bytes = WriteToBytes(v, true, false);
            // BCD: {0x00, 0x03}, then 'a','b','c'
            Assert.Equal(new byte[] { 0x00, 0x03, (byte)'a', (byte)'b', (byte)'c' }, bytes);
        }

        [Fact]
        public void Write_LLVAR_LengthTen_TwoDigitAsciiHeader()
        {
            var v = new IsoValue(IsoType.LLVAR, "1234567890");
            var bytes = WriteToBytes(v, false, false);
            // "10" + "1234567890"
            Assert.Equal(0x31, bytes[0]); // '1'
            Assert.Equal(0x30, bytes[1]); // '0'
        }

        [Fact]
        public void Write_LLBIN_Ascii_WritesHexLengthThenHex()
        {
            var raw = new sbyte[] { 0x01, 0x02 };
            var v = new IsoValue(IsoType.LLBIN, raw);
            var bytes = WriteToBytes(v, false, false);
            // Length = 2 bytes → hex header "04" (2*2=4 hex chars), then "0102"
            Assert.Equal(new byte[] { 0x30, 0x34, 0x30, 0x31, 0x30, 0x32 }, bytes);
        }

        [Fact]
        public void Write_LLBIN_Binary_WritesBcdLengthThenRawBytes()
        {
            var raw = new sbyte[] { 0x0a, 0x0b };
            var v = new IsoValue(IsoType.LLBIN, raw);
            var bytes = WriteToBytes(v, true, false);
            // BCD length = 0x02, then 0x0a, 0x0b
            Assert.Equal(new byte[] { 0x02, 0x0a, 0x0b }, bytes);
        }

        [Fact]
        public void Write_Numeric_Binary_WritesBcdEncoded()
        {
            var v = new IsoValue(IsoType.NUMERIC, "001234", 6);
            v.Encoding = Encoding.ASCII;
            var stream = new MemoryStream();
            v.Write(stream, true, false);
            var bytes = stream.ToArray();
            // BCD encode "001234" → {0x00, 0x12, 0x34}
            Assert.Equal(new byte[] { 0x00, 0x12, 0x34 }, bytes);
        }

        [Fact]
        public void Write_Binary_BinaryMode_SbyteArray_WritesRawBytes()
        {
            var raw = new sbyte[] { 0x0a, 0x0b, 0x0c };
            var v = new IsoValue(IsoType.BINARY, raw, 3);
            var bytes = WriteToBytes(v, true, false);
            Assert.Equal(new byte[] { 0x0a, 0x0b, 0x0c }, bytes);
        }

        [Fact]
        public void Write_Binary_BinaryMode_SbyteArray_PadsToLength()
        {
            var raw = new sbyte[] { 0x01, 0x02 };
            var v = new IsoValue(IsoType.BINARY, raw, 4);
            var bytes = WriteToBytes(v, true, false);
            // 2 bytes then 2 zero padding
            Assert.Equal(new byte[] { 0x01, 0x02, 0x00, 0x00 }, bytes);
        }

        [Fact]
        public void Write_Binary_BinaryMode_WithBinaryEncoder()
        {
            var enc = new BinaryEncoder(new sbyte[] { unchecked((sbyte)0xAA), unchecked((sbyte)0xBB) });
            var v = new IsoValue(IsoType.BINARY, "test", 2, enc);
            var bytes = WriteToBytes(v, true, false);
            Assert.Equal(new byte[] { 0xAA, 0xBB }, bytes);
        }

        [Fact]
        public void Write_Binary_BinaryMode_HexDecode()
        {
            // Value is a string → HexDecode it
            var v = new IsoValue(IsoType.BINARY, "0102", 2);
            var bytes = WriteToBytes(v, true, false);
            Assert.Equal(new byte[] { 0x01, 0x02 }, bytes);
        }

        [Fact]
        public void Write_Alpha_Ascii_WritesFormattedString()
        {
            var v = new IsoValue(IsoType.ALPHA, "hi", 4);
            var bytes = WriteToBytes(v, false, false);
            // "hi  " in ASCII
            Assert.Equal(new byte[] { (byte)'h', (byte)'i', (byte)' ', (byte)' ' }, bytes);
        }

        [Fact]
        public void Write_LLVAR_ForceStringEncoding_WritesPaddedLength()
        {
            var v = new IsoValue(IsoType.LLVAR, "abc");
            var bytes = WriteToBytes(v, false, true);
            // forceStringEncoding: writes "03" (2 ASCII chars) + "abc" (3 ASCII chars) = 5 bytes
            Assert.Equal(5, bytes.Length);
            // First 2 bytes are "03" in ASCII (0x30, 0x33)
            Assert.Equal(0x30, bytes[0]); // '0'
            Assert.Equal(0x33, bytes[1]); // '3'
            Assert.Equal((byte)'a', bytes[2]);
            Assert.Equal((byte)'b', bytes[3]);
            Assert.Equal((byte)'c', bytes[4]);
        }
    }
}
