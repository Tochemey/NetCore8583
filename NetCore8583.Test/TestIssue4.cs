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

﻿using System;
using System.IO;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test
{
    public class TestIssue4
    {
        [Fact]
        public void TestBinaryBitmap()
        {
            var mf = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mf, @"/Resources/issue4.xml");
            var bm = mf.GetMessageTemplate(0x800);
            bm.BinBitmap = true;
            var bb = bm.WriteToBuffer(2);
            Assert.Equal(62, bb.Length); //"Wrong message length for new BIN"

            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            foreach (var @sbyte in bb) binWriter.Write(@sbyte);
            var binReader = new BinaryReader(memStream);
            memStream.Position = 0;
            var buf = binReader.ReadBytes(2);
            Array.Reverse(buf); // due to the Big Endianness of Java
            Assert.Equal(60,
                BitConverter.ToInt16(buf,
                    0));

            var mfp = new MessageFactory<IsoMessage> {UseBinaryBitmap = true};
            ConfigParser.ConfigureFromClasspathConfig(mfp, @"/Resources/issue4.xml");

            var buf2 = binReader.ReadBytes((int) (memStream.Length - memStream.Position));
            bm = mfp.ParseMessage(buf2.ToInt8(), 0);
            Assert.True(bm.BinBitmap, "Parsed message should have binary bitmap flag set");
            Assert.False(bm.Binary);
            var bbp = bm.WriteToBuffer(2);
            Assert.Equal(bb,
                bbp); // "Parsed-reencoded BIN differs from original"
        }

        [Fact]
        public void TestTextBitmap()
        {
            var tmf = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(tmf,
                @"/Resources/issue4.xml");
            var tm = tmf.NewMessage(0x800);
            var bb = tm.WriteToBuffer(2);
            Assert.Equal(70,
                bb.Length); //"Wrong message length for new TXT"

            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            foreach (var @sbyte in bb) binWriter.Write(@sbyte);

            var binReader = new BinaryReader(memStream);
            memStream.Position = 0;
            var buf = binReader.ReadBytes(2);
            Array.Reverse(buf); // due to the Big Endianness of Java

            Assert.Equal(68,
                BitConverter.ToInt16(buf,
                    0));

            var tmfp = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(tmfp,
                @"/Resources/issue4.xml");

            var buf2 = binReader.ReadBytes((int) (memStream.Length - memStream.Position));
            tm = tmfp.ParseMessage(buf2.ToInt8(),
                0);

            var bbp = tm.WriteToBuffer(2);
            Assert.Equal(bb,
                bbp); // "Parsed-reencoded TXT differs from original"
        }
    }
}