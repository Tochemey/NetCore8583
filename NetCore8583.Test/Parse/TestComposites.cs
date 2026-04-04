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

﻿using System.Text;
using NetCore8583.Codecs;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class TestComposites
    {
        private readonly string textData = "One  03Two00999X";

        private readonly sbyte[] binaryData =
        {
            (sbyte) 'O', (sbyte) 'n', (sbyte) 'e', (sbyte) ' ', (sbyte) ' ', 3, (sbyte) 'T', (sbyte) 'w', (sbyte) 'o',
            0, 9, unchecked((sbyte) 0x99), (sbyte) 'X'
        };

        [Fact]
        public void TestDecodeBinary()
        {
            var dec = new CompositeField()
                .AddParser(new AlphaParseInfo(5))
                .AddParser(new LlvarParseInfo())
                .AddParser(new NumericParseInfo(5))
                .AddParser(new AlphaParseInfo(1));

            var f = (CompositeField) dec.DecodeBinaryField(binaryData, 0, binaryData.Length);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
            Assert.Equal("One  ", f.Values[0].Value);
            Assert.Equal("Two", f.Values[1].Value);
            Assert.Equal(999L, f.Values[2].Value);
            Assert.Equal("X", f.Values[3].Value);
        }

        [Fact]
        public void TestDecodeBinaryWithOffset()
        {
            var dec = new CompositeField()
                .AddParser(new LlvarParseInfo())
                .AddParser(new NumericParseInfo(5))
                .AddParser(new AlphaParseInfo(1));
            var offset = 5;
            var f = (CompositeField) dec.DecodeBinaryField(binaryData, offset, binaryData.Length - offset);
            Assert.NotNull(f);
            Assert.Equal(3, f.Values.Count);
            Assert.Equal("Two", f.Values[0].Value);
            Assert.Equal(999L, f.Values[1].Value);
            Assert.Equal("X", f.Values[2].Value);
        }

        [Fact]
        public void TestDecodeText()
        {
            var dec = new CompositeField()
                .AddParser(new AlphaParseInfo(5))
                .AddParser(new LlvarParseInfo())
                .AddParser(new NumericParseInfo(5))
                .AddParser(new AlphaParseInfo(1));

            var f = (CompositeField) dec.DecodeField(textData);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
            Assert.Equal("One  ", f.Values[0].Value);
            Assert.Equal("Two", f.Values[1].Value);
            Assert.Equal("00999", f.Values[2].Value);
            Assert.Equal("X", f.Values[3].Value);
        }

        [Fact]
        public void TestEncodeBinary()
        {
            var f = new CompositeField()
                .AddValue(new IsoValue(IsoType.ALPHA, "One", 5));
            Assert.Equal(new sbyte[] {(sbyte) 'O', (sbyte) 'n', (sbyte) 'e', 32, 32}, f.EncodeBinaryField(f));
            f.AddValue(new IsoValue(IsoType.LLVAR, "Two"));
            Assert.Equal(
                new sbyte[]
                {
                    (sbyte) 'O', (sbyte) 'n', (sbyte) 'e', (sbyte) ' ', (sbyte) ' ', 3, (sbyte) 'T', (sbyte) 'w',
                    (sbyte) 'o'
                },
                f.EncodeBinaryField(f));
            f.AddValue(new IsoValue(IsoType.NUMERIC, 999L, 5));
            f.AddValue(new IsoValue(IsoType.ALPHA, "X", 1));
            Assert.Equal(binaryData, f.EncodeBinaryField(f));
        }

        [Fact]
        public void TestEncodeText()
        {
            var f = new CompositeField();
            f.AddValue(new IsoValue(IsoType.ALPHA, "One", 5));
            f.Values[0].Encoding = Encoding.UTF8;
            Assert.Equal("One  ", f.EncodeField(f));
            f.AddValue("Two", null, IsoType.LLVAR, 0);
            f.Values[1].Encoding = Encoding.UTF8;
            Assert.Equal("One  03Two", f.EncodeField(f));
            f.AddValue(999, null, IsoType.NUMERIC, 5);
            f.Values[2].Encoding = Encoding.UTF8;
            Assert.Equal("One  03Two00999", f.EncodeField(f));
            f.AddValue("X", null, IsoType.ALPHA, 1);
            Assert.Equal(textData, f.EncodeField(f));
        }
    }
}