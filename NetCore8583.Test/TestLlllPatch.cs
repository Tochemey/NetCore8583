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

﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test
{
    public class TestLlllPatch
    {
        public TestLlllPatch()
        {
            mfact.SetConfigPath(@"/Resources/issue50.xml");
            mfact.AssignDate = false;
        }

        private readonly MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();

        [Theory]
        [InlineData(5)]
        [InlineData(50)]
        [InlineData(500)]
        [InlineData(5000)]
        public void TestParsingLength(int fieldLength)
        {
            // prepare
            var llllvar = MakeLlllVar(fieldLength);
            var sb = new StringBuilder();
            sb.Append("01004000000000000000").Append(fieldLength.ToString("D4")).Append(llllvar);

            // parse
            var m = mfact.ParseMessage(sb.ToString().GetSignedBytes(),
                0);
            Assert.NotNull(m);
            var f2 = (string) m.GetObjectValue(2);
            Assert.Equal(llllvar,
                f2);
            Assert.Equal(fieldLength,
                f2.Length);
            //Encode
            m = mfact.NewMessage(0x100);
            m.IsoHeader = null;
            m.SetValue(2,
                llllvar,
                IsoType.LLLLVAR,
                0);
            Assert.Equal(sb.ToString(),
                m.DebugString());
        }

        private string MakeLlllVar(int length)
        {
            var chars = new char[length];
            for (var i = 0; i < length; i++) chars[i] = 'a';

            return new string(chars);
        }

        private void SerialiseParse(int length)
        {
            // prepare
            var LLLLVar = MakeLlllVar(length);
            var m = mfact.NewMessage(0x100);
            m.SetValue(2,
                LLLLVar,
                IsoType.LLLLVAR,
                0);
            m.Binary = true;

            // write
            var bout = new List<sbyte>();
            m.Write(bout,
                2);

            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            foreach (var @sbyte in bout) binWriter.Write(@sbyte);

            // read
            var binReader = new BinaryReader(memStream);
            memStream.Position = 0;

            var buf = binReader.ReadBytes(2).ToInt8();
            Assert.NotEqual(buf,
                new sbyte[2]);

            var len = ((buf[0] & 0xff) << 8) | (buf[1] & 0xff);
            buf = binReader.ReadBytes(len).ToInt8();

            // parse
            mfact.UseBinaryMessages = true;
            m = mfact.ParseMessage(buf,
                mfact.GetIsoHeader(0x100).Length);
            Assert.NotNull(m);
            Assert.Equal(LLLLVar,
                m.GetObjectValue(2));
        }

        [Fact]
        public void testSerialiseParseLarge()
        {
            SerialiseParse(9919);
        }

        [Fact]
        public void TestSerialiseParseMedium()
        {
            SerialiseParse(258);
        }

        [Fact]
        public void TestSerialiseParseSmall()
        {
            SerialiseParse(88);
        }
    }
}