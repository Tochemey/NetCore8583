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
using NetCore8583.Tlv;
using Xunit;

namespace NetCore8583.Test.Tlv
{
    public class TestTlvRoundTrip
    {
        [Fact]
        public void RoundTripSingleByteTag()
        {
            var original = new byte[] { 0x82, 0x02, 0x19, 0x80 };
            var tags = TlvParser.Parse(original);

            var rebuilt = new TlvBuilder();
            foreach (var tag in tags) rebuilt.AddTag(tag);
            var result = rebuilt.Build();

            Assert.Equal(original, result);
        }

        [Fact]
        public void RoundTripMultiByteTag()
        {
            var original = new byte[]
            {
                0x9F, 0x26, 0x08,
                0xA1, 0xB2, 0xC3, 0xD4, 0xE5, 0xF6, 0x07, 0x08
            };

            var tags = TlvParser.Parse(original);
            var rebuilt = new TlvBuilder();
            foreach (var tag in tags) rebuilt.AddTag(tag);

            Assert.Equal(original, rebuilt.Build());
        }

        [Fact]
        public void RoundTripMultipleTags()
        {
            var original = new byte[]
            {
                0x9A, 0x03, 0x23, 0x01, 0x15,
                0x9C, 0x01, 0x00,
                0x5F, 0x2A, 0x02, 0x08, 0x40,
                0x9F, 0x26, 0x08, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x82, 0x02, 0x19, 0x80,
                0x95, 0x05, 0x00, 0x00, 0x08, 0x00, 0x00
            };

            var tags = TlvParser.Parse(original);
            Assert.Equal(6, tags.Count);

            var rebuilt = new TlvBuilder();
            foreach (var tag in tags) rebuilt.AddTag(tag);

            Assert.Equal(original, rebuilt.Build());
        }

        [Fact]
        public void RoundTripConstructedTag()
        {
            var inner = new byte[]
            {
                0x9F, 0x27, 0x01, 0x80,
                0x9F, 0x36, 0x02, 0x00, 0x01
            };

            var outer = new byte[1 + 1 + inner.Length];
            outer[0] = 0x70; // constructed
            outer[1] = (byte)inner.Length;
            Array.Copy(inner, 0, outer, 2, inner.Length);

            var tags = TlvParser.Parse(outer);
            Assert.Single(tags);

            var nested = tags[0].GetNestedTags();
            Assert.Equal(2, nested.Count);

            // Rebuild outer
            var rebuiltInner = new TlvBuilder();
            foreach (var n in nested) rebuiltInner.AddTag(n);
            var innerBytes = rebuiltInner.Build();

            var rebuiltOuter = new TlvBuilder();
            rebuiltOuter.AddTag("70", innerBytes);

            Assert.Equal(outer, rebuiltOuter.Build());
        }

        [Fact]
        public void RoundTripLongFormLength()
        {
            var value = new byte[200];
            new Random(42).NextBytes(value);

            var built = new TlvBuilder()
                .AddTag("9F10", value)
                .Build();

            var parsed = TlvParser.Parse(built);
            Assert.Single(parsed);
            Assert.Equal("9F10", parsed[0].Tag);
            Assert.Equal(200, parsed[0].Length);
            Assert.Equal(value, parsed[0].Value);

            // Re-encode
            var reBuilt = new TlvBuilder();
            foreach (var tag in parsed) reBuilt.AddTag(tag);
            Assert.Equal(built, reBuilt.Build());
        }

        [Fact]
        public void RoundTripThreeByteTag()
        {
            var original = new byte[] { 0xDF, 0x81, 0x01, 0x02, 0xAA, 0xBB };

            var tags = TlvParser.Parse(original);
            Assert.Single(tags);
            Assert.Equal("DF8101", tags[0].Tag);

            var rebuilt = new TlvBuilder();
            foreach (var tag in tags) rebuilt.AddTag(tag);
            Assert.Equal(original, rebuilt.Build());
        }

        [Fact]
        public void RoundTripFullEmvData()
        {
            var hex = "9F2608A1B2C3D4E5F60708" +
                      "9F270180" +
                      "9F100706010A03A4B000" +
                      "9F3704DEADBEEF" +
                      "9F36020042" +
                      "95050000080000" +
                      "9A03260401" +
                      "9C0100" +
                      "5F2A020840" +
                      "82021980" +
                      "9F1A020840" +
                      "9F0306000000000000" +
                      "9F3303E0F0C8" +
                      "9F3403020000" +
                      "9F350122" +
                      "9F0902008C" +
                      "8407A0000000041010";

            var original = Convert.FromHexString(hex);
            var tags = TlvParser.Parse(original);
            Assert.Equal(17, tags.Count);

            var rebuilt = new TlvBuilder();
            foreach (var tag in tags) rebuilt.AddTag(tag);
            Assert.Equal(original, rebuilt.Build());
        }
    }
}
