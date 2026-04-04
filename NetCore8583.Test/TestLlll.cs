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

namespace NetCore8583.Test
{
    public class TestLlll
    {
        public TestLlll()
        {
            mfact.SetConfigPath(@"/Resources/issue36.xml");
            mfact.AssignDate = false;
        }

        private readonly MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();

        [Fact]
        public void TestNewMessage()
        {
            var m = mfact.NewMessage(0x200);
            m.SetValue(2, "Variable length text", IsoType.LLLLVAR, 0);
            m.SetValue(3, "FFFF", IsoType.LLLLBIN, 0);
            Assert.Equal("020060000000000000000020Variable length text0004FFFF", m.DebugString());
            m.Binary = true;
            m.SetValue(2, "XX", IsoType.LLLLVAR, 0);
            m.SetValue(3, new[] {unchecked((sbyte) 0xff)}, IsoType.LLLLBIN, 0);
            Assert.Equal(new sbyte[]
            {
                2, 0, 0x60, 0, 0, 0, 0, 0, 0, 0,
                0, 2, (sbyte) 'X', (sbyte) 'X', 0, 1, unchecked((sbyte) 0xff)
            }, m.WriteData());
        }

        [Fact]
        public void TestParsing()
        {
            var m = mfact.ParseMessage("010060000000000000000001X0002FF".GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal(new[] {unchecked((sbyte) 0xff)}, (sbyte[]) m.GetObjectValue(3));
            mfact.UseBinaryMessages = true;
            m = mfact.ParseMessage(new byte[]
            {
                1, 0, 0x60, 0, 0, 0, 0, 0, 0, 0,
                0, 2, (byte) 'X', (byte) 'X', 0, 1, 0xff
            }.ToInt8(), 0);
            Assert.NotNull(m);
            Assert.Equal("XX", m.GetObjectValue(2));
            Assert.Equal(new[] {unchecked((sbyte) 0xff)}, (sbyte[]) m.GetObjectValue(3));
        }

        [Fact]
        public void TestL4bin()
        {
            sbyte[] fieldData = new sbyte[1000];
            mfact.UseBinaryMessages = true;
            IsoMessage m = mfact.NewMessage(0x100);
            m.SetValue(3, fieldData, IsoType.LLLLBIN, 0);
            fieldData = m.WriteData();
            //2 for message header
            //8 bitmap
            //3 for field 2 (from template)
            //1002 for field 3
            Assert.Equal(1015, fieldData.Length);
            m = mfact.ParseMessage(fieldData, 0);
            Assert.True(m.HasField(3));
            fieldData = m.GetObjectValue(3) as sbyte[];
            Assert.Equal(1000, fieldData.Length);
        }
        
        [Fact]
        public void TestTemplate()
        {
            var m = mfact.NewMessage(0x100);
            Assert.Equal("010060000000000000000001X0002FF", m.DebugString());
            m.Binary = true;
            Assert.Equal(new sbyte[]
            {
                1, 0, 0x60, 0, 0, 0, 0, 0, 0, 0,
                0, 1, (sbyte) 'X', 0, 1, unchecked((sbyte) 0xff)
            }, m.WriteData());
        }
    }
}