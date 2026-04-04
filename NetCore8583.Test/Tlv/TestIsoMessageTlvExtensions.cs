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
using System.Collections.Generic;
using NetCore8583.Tlv;
using Xunit;

namespace NetCore8583.Test.Tlv
{
    public class TestIsoMessageTlvExtensions
    {
        [Fact]
        public void SetAndGetTlvTags()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            var tags = new List<TlvTag>
            {
                new("9F26", 8, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }),
                new("9F27", 1, new byte[] { 0x80 }),
                new("82", 2, new byte[] { 0x19, 0x80 })
            };

            msg.SetTlvTags(tags);

            Assert.True(msg.HasField(55));

            var retrieved = msg.GetTlvTags();
            Assert.NotNull(retrieved);
            Assert.Equal(3, retrieved.Count);
            Assert.Equal("9F26", retrieved[0].Tag);
            Assert.Equal("9F27", retrieved[1].Tag);
            Assert.Equal("82", retrieved[2].Tag);
        }

        [Fact]
        public void GetTlvTagsFromByteArrayField()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            var tlvBytes = new byte[] { 0x82, 0x02, 0x19, 0x80 };
            msg.SetValue(55, tlvBytes, IsoType.LLLBIN, tlvBytes.Length);

            var tags = msg.GetTlvTags();

            Assert.NotNull(tags);
            Assert.Single(tags);
            Assert.Equal("82", tags[0].Tag);
        }

        [Fact]
        public void GetTlvTagsFromHexStringField()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            msg.SetValue(55, "82021980", IsoType.LLLVAR, 0);

            var tags = msg.GetTlvTags();

            Assert.NotNull(tags);
            Assert.Single(tags);
            Assert.Equal("82", tags[0].Tag);
        }

        [Fact]
        public void GetTlvTagsReturnsNullWhenFieldNotPresent()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            Assert.Null(msg.GetTlvTags());
        }

        [Fact]
        public void GetTlvBytesRoundTrip()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            var tags = new List<TlvTag>
            {
                new("9F27", 1, new byte[] { 0x80 }),
                new("82", 2, new byte[] { 0x19, 0x80 })
            };

            msg.SetTlvTags(tags);

            var bytes = msg.GetTlvBytes();
            Assert.NotNull(bytes);

            var reparsed = TlvParser.Parse(bytes);
            Assert.Equal(2, reparsed.Count);
            Assert.Equal("9F27", reparsed[0].Tag);
            Assert.Equal("82", reparsed[1].Tag);
        }

        [Fact]
        public void FindTlvTag()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            var tags = new List<TlvTag>
            {
                new("9F26", 8, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }),
                new("9F27", 1, new byte[] { 0x80 }),
                new("82", 2, new byte[] { 0x19, 0x80 })
            };
            msg.SetTlvTags(tags);

            var found = msg.FindTlvTag("9F27");
            Assert.NotNull(found);
            Assert.Equal("9F27", found.Tag);
            Assert.Equal(new byte[] { 0x80 }, found.Value);
        }

        [Fact]
        public void FindTlvTagCaseInsensitive()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            var tags = new List<TlvTag>
            {
                new("9F26", 8, new byte[8])
            };
            msg.SetTlvTags(tags);

            Assert.NotNull(msg.FindTlvTag("9f26"));
            Assert.NotNull(msg.FindTlvTag("9F26"));
        }

        [Fact]
        public void FindTlvTagReturnsNullWhenNotFound()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            var tags = new List<TlvTag>
            {
                new("82", 2, new byte[] { 0x19, 0x80 })
            };
            msg.SetTlvTags(tags);

            Assert.Null(msg.FindTlvTag("9F26"));
        }

        [Fact]
        public void SetTlvTagsOnCustomFieldNumber()
        {
            var msg = new IsoMessage { Type = 0x0100 };
            var tags = new List<TlvTag>
            {
                new("82", 2, new byte[] { 0x19, 0x80 })
            };

            msg.SetTlvTags(tags, fieldNumber: 62);

            Assert.True(msg.HasField(62));
            Assert.False(msg.HasField(55));

            var retrieved = msg.GetTlvTags(62);
            Assert.NotNull(retrieved);
            Assert.Single(retrieved);
        }
    }
}
