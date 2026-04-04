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
using System.Text;
using NetCore8583.Builder;
using NetCore8583.Codecs;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Builder
{
    /// <summary>
    /// Tests for CompositeFieldBuilder, exercising error paths and valid configurations
    /// through the public API. The internal BuildValues/BuildParsers are exercised via
    /// MessageFactoryBuilder.
    /// </summary>
    public class TestCompositeFieldBuilder
    {
        // ── SubField error cases ──────────────────────────────────────────────────

        [Fact]
        public void SubField_NeedsLengthType_WithoutLength_Throws()
        {
            var builder = new CompositeFieldBuilder();
            Assert.Throws<ArgumentException>(() => builder.SubField(IsoType.ALPHA, "value"));
            Assert.Throws<ArgumentException>(() => builder.SubField(IsoType.NUMERIC, "123"));
            Assert.Throws<ArgumentException>(() => builder.SubField(IsoType.BINARY, "0102"));
        }

        [Fact]
        public void SubField_NeedsLengthType_WithEncoderButNoLength_Throws()
        {
            var builder = new CompositeFieldBuilder();
            var enc = new DummyField();
            Assert.Throws<ArgumentException>(() => builder.SubField(IsoType.ALPHA, (object)"value", enc));
            Assert.Throws<ArgumentException>(() => builder.SubField(IsoType.NUMERIC, (object)"123", enc));
        }

        // ── SubParser error cases ─────────────────────────────────────────────────

        [Fact]
        public void SubParser_NeedsLengthType_WithoutLength_Throws()
        {
            var builder = new CompositeFieldBuilder();
            Assert.Throws<ArgumentException>(() => builder.SubParser(IsoType.ALPHA));
            Assert.Throws<ArgumentException>(() => builder.SubParser(IsoType.NUMERIC));
            Assert.Throws<ArgumentException>(() => builder.SubParser(IsoType.BINARY));
        }

        // ── Valid SubField calls (via MessageFactoryBuilder round-trip) ───────────

        [Fact]
        public void SubField_VariableLengthTypes_AcceptedWithoutLength()
        {
            // Should not throw for variable-length types
            var builder = new CompositeFieldBuilder();
            builder.SubField(IsoType.LLVAR, "hello");
            builder.SubField(IsoType.LLLVAR, "world");
            builder.SubField(IsoType.LLLLVAR, "data");
        }

        [Fact]
        public void SubField_FixedLengthType_WithLength_Accepted()
        {
            var builder = new CompositeFieldBuilder();
            builder.SubField(IsoType.ALPHA, "hello", 5);
            builder.SubField(IsoType.NUMERIC, "12345", 5);
            builder.SubField(IsoType.BINARY, "0102", 4);
        }

        [Fact]
        public void SubField_WithEncoder_FixedLength_Accepted()
        {
            var enc = new DummyField();
            var builder = new CompositeFieldBuilder();
            builder.SubField(IsoType.ALPHA, (object)"hello", 5, enc);
            builder.SubField(IsoType.LLVAR, (object)"world", enc);
        }

        [Fact]
        public void SubParser_VariableLengthTypes_Accepted()
        {
            var builder = new CompositeFieldBuilder();
            builder.SubParser(IsoType.LLVAR);
            builder.SubParser(IsoType.LLLVAR);
            builder.SubParser(IsoType.LLLLVAR);
        }

        [Fact]
        public void SubParser_FixedLengthType_WithLength_Accepted()
        {
            var builder = new CompositeFieldBuilder();
            builder.SubParser(IsoType.ALPHA, 5);
            builder.SubParser(IsoType.NUMERIC, 6);
        }

        // ── Chaining returns same builder ──────────────────────────────────────────

        [Fact]
        public void SubField_SupportsChaining()
        {
            var builder = new CompositeFieldBuilder();
            var result = builder
                .SubField(IsoType.ALPHA, "a", 1)
                .SubField(IsoType.LLVAR, "b")
                .SubField(IsoType.NUMERIC, "1", 1);
            Assert.Same(builder, result);
        }

        [Fact]
        public void SubParser_SupportsChaining()
        {
            var builder = new CompositeFieldBuilder();
            var result = builder
                .SubParser(IsoType.ALPHA, 5)
                .SubParser(IsoType.LLVAR)
                .SubParser(IsoType.NUMERIC, 6);
            Assert.Same(builder, result);
        }

        // ── End-to-end via MessageFactoryBuilder ──────────────────────────────────

        [Fact]
        public void BuildValues_ViaMessageFactoryBuilder_ProducesCorrectCompositeField()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithTemplate(0x0100, t => t
                    .CompositeField(10, IsoType.LLLVAR, cf => cf
                        .SubField(IsoType.ALPHA, "hello", 5)
                        .SubField(IsoType.LLVAR, "world")
                        .SubField(IsoType.NUMERIC, "123", 3)))
                .Build();

            var m = factory.NewMessage(0x0100);
            Assert.True(m.HasField(10));
            var f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal(3, f.Values.Count);
            Assert.Equal("hello", f.GetObjectValue(0));
            Assert.Equal("world", f.GetObjectValue(1));
            Assert.Equal("123", f.GetObjectValue(2));
        }

        [Fact]
        public void BuildParsers_ViaMessageFactoryBuilder_ParsesCompositeField()
        {
            // Use the same message format as TestMessageFactoryBuilder.TestCompositeFieldParseMap
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithParseMap(0x0100, p => p
                    .CompositeField(10, IsoType.LLLVAR, cf => cf
                        .SubParser(IsoType.ALPHA, 5)
                        .SubParser(IsoType.LLVAR)
                        .SubParser(IsoType.NUMERIC, 5)
                        .SubParser(IsoType.ALPHA, 1)))
                .Build();

            var msg = "01000040000000000000016one  03two12345.";
            var m = factory.ParseMessage(msg.GetSignedBytes(Encoding.ASCII), 0);
            Assert.NotNull(m);
            var f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
            Assert.Equal("one  ", f.GetObjectValue(0));
            Assert.Equal("two", f.GetObjectValue(1));
            Assert.Equal("12345", f.GetObjectValue(2));
            Assert.Equal(".", f.GetObjectValue(3));
        }

        // ── Dummy ICustomField ────────────────────────────────────────────────────

        private sealed class DummyField : ICustomField
        {
            public object DecodeField(string value) => value;
            public string EncodeField(object value) => value?.ToString();
        }
    }
}
