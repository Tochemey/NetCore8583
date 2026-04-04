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
using Xunit;

namespace NetCore8583.Test
{
    public class TestHeaders
    {
        public TestHeaders()
        {
            mf = new MessageFactory<IsoMessage>
            {
                Encoding = Encoding.UTF8
            };
            mf.SetConfigPath(@"/Resources/config.xml");
        }

        private readonly MessageFactory<IsoMessage> mf;

        [Fact]
        public void TestBinaryHeader()
        {
            var m = mf.NewMessage(0x280);
            Assert.NotNull(m.BinIsoHeader);
            var buf = m.WriteData();
            Assert.Equal(4 + 4 + 16 + 2, buf.Length);
            for (var i = 0; i < 4; i++) Assert.Equal(unchecked((sbyte)0xff), buf[i]);
            Assert.Equal(0x30, buf[4]);
            Assert.Equal(0x32, buf[5]);
            Assert.Equal(0x38, buf[6]);
            Assert.Equal(0x30, buf[7]);
            //Then parse and check the header is binary 0xffffffff
            m = mf.ParseMessage(buf, 4, true);
            Assert.Null(m.IsoHeader);
            buf = m.BinIsoHeader;
            Assert.NotNull(buf);
            for (var i = 0; i < 4; i++) Assert.Equal(unchecked((sbyte)0xff), buf[i]);
            Assert.Equal(0x280, m.Type);
            Assert.True(m.HasField(3));
        }
    }
}