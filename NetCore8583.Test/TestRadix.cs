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

using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test
{
    public class TestRadix
    {
        private readonly MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();

        public TestRadix()
        {
            mfact.SetConfigPath(@"/Resources/radix.xml");
            mfact.ForceStringEncoding = true;
        }

        [Fact]
        public void TestParseLengthWithRadix10()
        {
            // Given
            var input = "0100" +  // MTI
                        "7000000000000000" + // bitmap
                        "10" + "ABCDEFGHIJ" + // F2 length (10 = 10) + value
                        "26" + "01234567890123456789012345" +  // F3 length (26 = 26) + value
                        "ZZZZZZZZ"; // F4
            
            // When
            IsoMessage m = mfact.ParseMessage(input.GetSignedBytes(), 0);
            
            // Then
            Assert.NotNull(m);
            Assert.Equal("ABCDEFGHIJ", m.GetObjectValue(2));
            Assert.Equal("01234567890123456789012345", HexCodec.HexEncode((sbyte[]) m.GetObjectValue(3), 0, 13));
            Assert.Equal("ZZZZZZZZ", m.GetObjectValue(4));
        }

        [Fact]
        public void TestParseLengthWithRadix16()
        {
            // Given
            mfact.Radix = 16;
            var input = "0100" +  // MTI
                        "7000000000000000" + // bitmap
                        "0A" + "ABCDEFGHIJ" +  // F2 length (0A = 10) + value
                        "1A" + "01234567890123456789012345" +   // F3 length (1A = 26) + value
                        "ZZZZZZZZ"; // F4
            
            // When
            IsoMessage m = mfact.ParseMessage(input.GetSignedBytes(), 0);
            
            // Then
            Assert.NotNull(m);
            Assert.Equal("ABCDEFGHIJ", m.GetObjectValue(2));
            Assert.Equal("01234567890123456789012345", HexCodec.HexEncode((sbyte[]) m.GetObjectValue(3), 0, 13));
            Assert.Equal("ZZZZZZZZ", m.GetObjectValue(4));
        }
    }
}