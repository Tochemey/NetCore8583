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

﻿using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestBcd
    {
        [Fact]
        public void TestDecoding()
        {
            var buf = new sbyte[2];
            Assert.Equal(0, Bcd.DecodeToLong(buf, 0, 1));
            Assert.Equal(0, Bcd.DecodeToLong(buf, 0, 2));
            Assert.Equal(0, Bcd.DecodeToLong(buf, 0, 3));
            Assert.Equal(0, Bcd.DecodeToLong(buf, 0, 4));
            buf[0] = 0x79;
            Assert.Equal(79, Bcd.DecodeToLong(buf, 0, 2));
            buf[0] = unchecked((sbyte) 0x80);
            Assert.Equal(80, Bcd.DecodeToLong(buf, 0, 2));
            buf[0] = unchecked((sbyte) 0x99);
            Assert.Equal(99, Bcd.DecodeToLong(buf, 0, 2));
            buf[0] = 1;
            Assert.Equal(100, Bcd.DecodeToLong(buf, 0, 4));
            buf[1] = 0x79;
            Assert.Equal(179, Bcd.DecodeToLong(buf, 0, 4));
            buf[1] = unchecked((sbyte) 0x99);
            Assert.Equal(199, Bcd.DecodeToLong(buf, 0, 4));
            buf[0] = 9;
            Assert.Equal(999, Bcd.DecodeToLong(buf, 0, 4));
        }

        [Fact]
        public void TestEncoding()
        {
            var buf = new sbyte[2];
            buf[0] = 1;
            buf[1] = 1;
            Bcd.Encode("00", buf);
            Assert.Equal(new byte[] {0, 1}.ToInt8(), buf);
            Bcd.Encode("79", buf);
            Assert.Equal(new byte[] {0x79, 1}.ToInt8(), buf);
            Bcd.Encode("80", buf);
            Assert.Equal(new byte[] {0x80, 1}.ToInt8(), buf);
            Bcd.Encode("99", buf);
            Assert.Equal(new byte[] {0x99, 1}.ToInt8(), buf);
            Bcd.Encode("100", buf);
            Assert.Equal(new byte[] {1, 0}.ToInt8(), buf);
            Bcd.Encode("779", buf);
            Assert.Equal(new byte[] {7, 0x79}.ToInt8(), buf);
            Bcd.Encode("999", buf);
            Assert.Equal(new byte[] {9, 0x99}.ToInt8(), buf);
        }
    }
}