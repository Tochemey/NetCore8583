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
    public class TestTlvBuilder
    {
        [Fact]
        public void BuildSingleTag()
        {
            var result = new TlvBuilder()
                .AddTag("82", new byte[] { 0x19, 0x80 })
                .Build();

            Assert.Equal(new byte[] { 0x82, 0x02, 0x19, 0x80 }, result);
        }

        [Fact]
        public void BuildMultiByteTag()
        {
            var value = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            var result = new TlvBuilder()
                .AddTag("9F26", value)
                .Build();

            var expected = new byte[2 + 1 + 8];
            expected[0] = 0x9F;
            expected[1] = 0x26;
            expected[2] = 0x08;
            Array.Copy(value, 0, expected, 3, 8);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void BuildMultipleTags()
        {
            var result = new TlvBuilder()
                .AddTag("9A", new byte[] { 0x23, 0x01, 0x15 })
                .AddTag("9C", new byte[] { 0x00 })
                .AddTag("5F2A", new byte[] { 0x08, 0x40 })
                .Build();

            var expected = new byte[]
            {
                0x9A, 0x03, 0x23, 0x01, 0x15,
                0x9C, 0x01, 0x00,
                0x5F, 0x2A, 0x02, 0x08, 0x40
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void BuildLongFormLengthOneByte()
        {
            var value = new byte[200];
            var result = new TlvBuilder()
                .AddTag("9F10", value)
                .Build();

            Assert.Equal(0x9F, result[0]);
            Assert.Equal(0x10, result[1]);
            Assert.Equal(0x81, result[2]); // long form marker
            Assert.Equal(0xC8, result[3]); // 200
            Assert.Equal(2 + 2 + 200, result.Length);
        }

        [Fact]
        public void BuildLongFormLengthTwoBytes()
        {
            var value = new byte[300];
            var result = new TlvBuilder()
                .AddTag("9F10", value)
                .Build();

            Assert.Equal(0x9F, result[0]);
            Assert.Equal(0x10, result[1]);
            Assert.Equal(0x82, result[2]);
            Assert.Equal(0x01, result[3]); // 300 >> 8
            Assert.Equal(0x2C, result[4]); // 300 & 0xFF
            Assert.Equal(2 + 3 + 300, result.Length);
        }

        [Fact]
        public void BuildEmptyValue()
        {
            var result = new TlvBuilder()
                .AddTag("9C", Array.Empty<byte>())
                .Build();

            Assert.Equal(new byte[] { 0x9C, 0x00 }, result);
        }

        [Fact]
        public void BuildFromTlvTag()
        {
            var tag = new TlvTag("9F27", 1, new byte[] { 0x80 }, "Cryptogram Information Data");
            var result = new TlvBuilder()
                .AddTag(tag)
                .Build();

            Assert.Equal(new byte[] { 0x9F, 0x27, 0x01, 0x80 }, result);
        }

        [Fact]
        public void BuildFromRawBytes()
        {
            var result = new TlvBuilder()
                .AddTag(new byte[] { 0x9F, 0x26 }, new byte[] { 0xAA, 0xBB })
                .Build();

            Assert.Equal(new byte[] { 0x9F, 0x26, 0x02, 0xAA, 0xBB }, result);
        }

        [Fact]
        public void BuildNoTagsReturnsEmpty()
        {
            var result = new TlvBuilder().Build();
            Assert.Empty(result);
        }

        [Fact]
        public void AddTagNullTagStringThrows()
        {
            Assert.Throws<ArgumentException>(() => new TlvBuilder().AddTag((string)null, new byte[] { 0x00 }));
        }

        [Fact]
        public void AddTagNullValueThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TlvBuilder().AddTag("82", null));
        }

        [Fact]
        public void AddTagEmptyTagStringThrows()
        {
            Assert.Throws<ArgumentException>(() => new TlvBuilder().AddTag("", new byte[] { 0x00 }));
        }

        [Fact]
        public void FluentChainingWorks()
        {
            var builder = new TlvBuilder();
            var returned = builder.AddTag("82", new byte[] { 0x19, 0x80 });
            Assert.Same(builder, returned);
        }
    }
}
