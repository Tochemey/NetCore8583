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
