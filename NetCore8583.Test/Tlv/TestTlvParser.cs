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
using System.Collections.Generic;
using NetCore8583.Tlv;
using Xunit;

namespace NetCore8583.Test.Tlv
{
    public class TestTlvParser
    {
        [Fact]
        public void ParseSingleByteTag()
        {
            // Tag 82 (Application Interchange Profile), 2 bytes value
            var data = new byte[] { 0x82, 0x02, 0x19, 0x80 };
            var tags = TlvParser.Parse(data);

            Assert.Single(tags);
            Assert.Equal("82", tags[0].Tag);
            Assert.Equal(2, tags[0].Length);
            Assert.Equal(new byte[] { 0x19, 0x80 }, tags[0].Value);
            Assert.Equal("Application Interchange Profile", tags[0].Description);
        }

        [Fact]
        public void ParseMultiByteTag()
        {
            // Tag 9F26 (Application Cryptogram), 8 bytes value
            var value = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            var data = new byte[2 + 1 + 8];
            data[0] = 0x9F;
            data[1] = 0x26;
            data[2] = 0x08;
            Array.Copy(value, 0, data, 3, 8);

            var tags = TlvParser.Parse(data);

            Assert.Single(tags);
            Assert.Equal("9F26", tags[0].Tag);
            Assert.Equal(8, tags[0].Length);
            Assert.Equal(value, tags[0].Value);
            Assert.Equal("Application Cryptogram", tags[0].Description);
        }

        [Fact]
        public void ParseMultipleTags()
        {
            // 9A (Transaction Date, 3 bytes) + 9C (Transaction Type, 1 byte) + 5F2A (Currency Code, 2 bytes)
            var data = new byte[]
            {
                0x9A, 0x03, 0x23, 0x01, 0x15,       // 9A = 230115
                0x9C, 0x01, 0x00,                     // 9C = 00
                0x5F, 0x2A, 0x02, 0x08, 0x40          // 5F2A = 0840
            };

            var tags = TlvParser.Parse(data);

            Assert.Equal(3, tags.Count);

            Assert.Equal("9A", tags[0].Tag);
            Assert.Equal(3, tags[0].Length);
            Assert.Equal("Transaction Date", tags[0].Description);

            Assert.Equal("9C", tags[1].Tag);
            Assert.Equal(1, tags[1].Length);
            Assert.Equal("Transaction Type", tags[1].Description);

            Assert.Equal("5F2A", tags[2].Tag);
            Assert.Equal(2, tags[2].Length);
            Assert.Equal("Transaction Currency Code", tags[2].Description);
        }

        [Fact]
        public void ParseMultiByteLengthShortForm()
        {
            // Tag 95 (TVR), length = 5 (short form, single byte)
            var data = new byte[] { 0x95, 0x05, 0x00, 0x00, 0x08, 0x00, 0x00 };
            var tags = TlvParser.Parse(data);

            Assert.Single(tags);
            Assert.Equal("95", tags[0].Tag);
            Assert.Equal(5, tags[0].Length);
        }

        [Fact]
        public void ParseMultiByteLengthLongFormOneByte()
        {
            // Tag 9F10 with length = 200 (0x81, 0xC8)
            var value = new byte[200];
            new Random(42).NextBytes(value);
            var data = new byte[2 + 2 + 200];
            data[0] = 0x9F;
            data[1] = 0x10;
            data[2] = 0x81; // long form, 1 subsequent byte
            data[3] = 0xC8; // 200
            Array.Copy(value, 0, data, 4, 200);

            var tags = TlvParser.Parse(data);

            Assert.Single(tags);
            Assert.Equal("9F10", tags[0].Tag);
            Assert.Equal(200, tags[0].Length);
            Assert.Equal(value, tags[0].Value);
        }

        [Fact]
        public void ParseMultiByteLengthLongFormTwoBytes()
        {
            // Tag 9F10 with length = 300 (0x82, 0x01, 0x2C)
            var value = new byte[300];
            new Random(42).NextBytes(value);
            var data = new byte[2 + 3 + 300];
            data[0] = 0x9F;
            data[1] = 0x10;
            data[2] = 0x82;
            data[3] = 0x01;
            data[4] = 0x2C;
            Array.Copy(value, 0, data, 5, 300);

            var tags = TlvParser.Parse(data);

            Assert.Single(tags);
            Assert.Equal("9F10", tags[0].Tag);
            Assert.Equal(300, tags[0].Length);
        }

        [Fact]
        public void ParseConstructedTag()
        {
            // Tag 70 (EMV Proprietary Template) is constructed (bit 6 set: 0x70 = 0111_0000)
            // Contains nested: 9F27 (1 byte) + 9F36 (2 bytes)
            var innerData = new byte[]
            {
                0x9F, 0x27, 0x01, 0x80,             // CID = 0x80
                0x9F, 0x36, 0x02, 0x00, 0x01         // ATC = 0001
            };

            var data = new byte[1 + 1 + innerData.Length];
            data[0] = 0x70;
            data[1] = (byte)innerData.Length;
            Array.Copy(innerData, 0, data, 2, innerData.Length);

            var tags = TlvParser.Parse(data);

            Assert.Single(tags);
            Assert.Equal("70", tags[0].Tag);
            Assert.True(tags[0].IsConstructed);

            var nested = tags[0].GetNestedTags();
            Assert.Equal(2, nested.Count);
            Assert.Equal("9F27", nested[0].Tag);
            Assert.Equal("Cryptogram Information Data", nested[0].Description);
            Assert.Equal("9F36", nested[1].Tag);
            Assert.Equal("Application Transaction Counter (ATC)", nested[1].Description);
        }

        [Fact]
        public void ParseSkipsPaddingBytes()
        {
            // 0x00 padding + tag 82 + 0xFF padding + tag 95
            var data = new byte[]
            {
                0x00,                                   // padding
                0x82, 0x02, 0x19, 0x80,                 // AIP
                0xFF,                                   // padding
                0x95, 0x05, 0x00, 0x00, 0x08, 0x00, 0x00 // TVR
            };

            var tags = TlvParser.Parse(data);

            Assert.Equal(2, tags.Count);
            Assert.Equal("82", tags[0].Tag);
            Assert.Equal("95", tags[1].Tag);
        }

        [Fact]
        public void ParseEmptyDataReturnsEmptyList()
        {
            var tags = TlvParser.Parse(Array.Empty<byte>());
            Assert.Empty(tags);
        }

        [Fact]
        public void ParseNullDataThrows()
        {
            Assert.Throws<ArgumentNullException>(() => TlvParser.Parse((byte[])null));
        }

        [Fact]
        public void ParseTruncatedTagThrows()
        {
            // Multi-byte tag that starts but data ends before tag is complete
            var data = new byte[] { 0x9F };
            Assert.Throws<FormatException>(() => TlvParser.Parse(data));
        }

        [Fact]
        public void ParseTruncatedLengthThrows()
        {
            // Tag is complete but length field is truncated
            var data = new byte[] { 0x82, 0x82 }; // long form says 2 bytes follow, but none do
            Assert.Throws<FormatException>(() => TlvParser.Parse(data));
        }

        [Fact]
        public void ParseTruncatedValueThrows()
        {
            // Tag + length declare 5 bytes but only 2 are present
            var data = new byte[] { 0x95, 0x05, 0x01, 0x02 };
            Assert.Throws<FormatException>(() => TlvParser.Parse(data));
        }

        [Fact]
        public void ParseRealWorldEmvField55()
        {
            // Simulated real EMV field 55 data
            var hex = "9F2608" + "A1B2C3D4E5F60708" +
                      "9F2701" + "80" +
                      "9F1007" + "06010A03A4B000" +
                      "9F3704" + "DEADBEEF" +
                      "9F3602" + "0042" +
                      "9505" + "0000080000" +
                      "9A03" + "260401" +
                      "9C01" + "00" +
                      "5F2A02" + "0840" +
                      "8202" + "1980" +
                      "9F1A02" + "0840" +
                      "9F0306" + "000000000000" +
                      "9F3303" + "E0F0C8" +
                      "9F3403" + "020000" +
                      "9F3501" + "22" +
                      "9F0902" + "008C" +
                      "8407" + "A0000000041010";

            var data = Convert.FromHexString(hex);
            var tags = TlvParser.Parse(data);

            Assert.Equal(17, tags.Count);

            Assert.Equal("9F26", tags[0].Tag);
            Assert.Equal("Application Cryptogram", tags[0].Description);

            Assert.Equal("84", tags[16].Tag);
            Assert.Equal("Dedicated File (DF) Name", tags[16].Description);
        }

        [Fact]
        public void ParseThreeByteTag()
        {
            // Construct a 3-byte tag: first byte has low 5 bits = 0x1F,
            // second byte has bit 8 set (continuation), third byte has bit 8 clear (last)
            // Tag = DF8101, length 2, value = 0x0102
            var data = new byte[] { 0xDF, 0x81, 0x01, 0x02, 0x01, 0x02 };

            var tags = TlvParser.Parse(data);

            Assert.Single(tags);
            Assert.Equal("DF8101", tags[0].Tag);
            Assert.Equal(2, tags[0].Length);
            Assert.Equal(new byte[] { 0x01, 0x02 }, tags[0].Value);
        }

        [Fact]
        public void GetNestedTagsThrowsOnPrimitive()
        {
            var tag = new TlvTag("9F26", 8, new byte[8]);
            Assert.False(tag.IsConstructed);
            Assert.Throws<InvalidOperationException>(() => tag.GetNestedTags());
        }
    }
}
