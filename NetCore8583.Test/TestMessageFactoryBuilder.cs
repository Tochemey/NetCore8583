using System.Text;
using NetCore8583.Builder;
using NetCore8583.Codecs;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test
{
    public class TestMessageFactoryBuilder
    {
        // ──────────────────────────────────────────────────────────────────
        //  Basic template tests
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestBasicTemplate()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithTemplate(0x0200, t => t
                    .Field(3, IsoType.NUMERIC, "650000", 6)
                    .Field(32, IsoType.LLVAR, "456")
                    .Field(49, IsoType.ALPHA, "484", 3))
                .Build();

            var m = factory.NewMessage(0x0200);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(32));
            Assert.True(m.HasField(49));
            Assert.Equal("650000", m.GetObjectValue(3));
            Assert.Equal("456", m.GetObjectValue(32));
            Assert.Equal("484", m.GetObjectValue(49));
        }

        [Fact]
        public void TestTemplateInheritance()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithTemplate(0x0200, t => t
                    .Field(3, IsoType.NUMERIC, "650000", 6)
                    .Field(32, IsoType.LLVAR, "456")
                    .Field(49, IsoType.ALPHA, "484", 3)
                    .Field(102, IsoType.LLVAR, "ABCD"))
                .WithTemplate(0x0400, t => t
                    .Extends(0x0200)
                    .Field(90, IsoType.ALPHA, "BLA", 42)
                    .Exclude(102))
                .Build();

            var m200 = factory.GetMessageTemplate(0x0200);
            var m400 = factory.GetMessageTemplate(0x0400);

            Assert.NotNull(m200);
            Assert.NotNull(m400);

            // Inherited fields
            Assert.True(m400.HasField(3));
            Assert.True(m400.HasField(32));
            Assert.True(m400.HasField(49));
            Assert.Equal(m200.GetField(3).Value, m400.GetField(3).Value);

            // New field in 0x0400
            Assert.False(m200.HasField(90));
            Assert.True(m400.HasField(90));

            // Excluded field
            Assert.True(m200.HasField(102));
            Assert.False(m400.HasField(102));
        }

        // ──────────────────────────────────────────────────────────────────
        //  Basic parse map tests
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestBasicParseMap()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithParseMap(0x0800, p => p
                    .Field(3, IsoType.ALPHA, 6)
                    .Field(12, IsoType.DATE4)
                    .Field(17, IsoType.DATE4))
                .Build();

            var s800 = "0800201080000000000012345611251125";
            var m = factory.ParseMessage(s800.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.True(m.HasField(17));
        }

        [Fact]
        public void TestParseMapInheritance()
        {
            // Replicate the config.xml parse guide inheritance:
            // 0810 extends 0800, excludes field 17, adds field 39
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithParseMap(0x0800, p => p
                    .Field(3, IsoType.ALPHA, 6)
                    .Field(12, IsoType.DATE4)
                    .Field(17, IsoType.DATE4))
                .WithParseMap(0x0810, p => p
                    .Extends(0x0800)
                    .Exclude(17)
                    .Field(39, IsoType.ALPHA, 2))
                .Build();

            var s800 = "0800201080000000000012345611251125";
            var s810 = "08102010000002000000123456112500";

            var m = factory.ParseMessage(s800.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.True(m.HasField(17));
            Assert.False(m.HasField(39));

            m = factory.ParseMessage(s810.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.False(m.HasField(17));
            Assert.True(m.HasField(39));
        }

        // ──────────────────────────────────────────────────────────────────
        //  Multilevel inheritance (issue34 equivalent)
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestMultilevelExtendParseGuides()
        {
            // Replicates issue34.xml programmatically
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithParseMap(0x0200, p => p
                    .Field(2, IsoType.LLVAR)
                    .Field(3, IsoType.NUMERIC, 6)
                    .Field(4, IsoType.NUMERIC, 12)
                    .Field(7, IsoType.DATE10)
                    .Field(11, IsoType.NUMERIC, 6)
                    .Field(12, IsoType.TIME)
                    .Field(13, IsoType.DATE4)
                    .Field(15, IsoType.DATE4)
                    .Field(18, IsoType.NUMERIC, 4)
                    .Field(32, IsoType.LLVAR)
                    .Field(37, IsoType.NUMERIC, 12)
                    .Field(41, IsoType.ALPHA, 8)
                    .Field(42, IsoType.ALPHA, 15)
                    .Field(48, IsoType.LLLVAR)
                    .Field(49, IsoType.ALPHA, 3))
                .WithParseMap(0x0210, p => p
                    .Extends(0x0200)
                    .Field(39, IsoType.ALPHA, 2)
                    .Field(62, IsoType.LLLVAR))
                .WithParseMap(0x0400, p => p
                    .Extends(0x0200)
                    .Field(62, IsoType.LLLVAR))
                .WithParseMap(0x0410, p => p
                    .Extends(0x0400)
                    .Field(39, IsoType.ALPHA, 2)
                    .Field(61, IsoType.LLLVAR))
                .Build();

            var m200 = "0200422000000880800001X1231235959123456101010202020TERMINAL484";
            var m210 = "0210422000000A80800001X123123595912345610101020202099TERMINAL484";
            var m400 = "0400422000000880800401X1231235959123456101010202020TERMINAL484001X";
            var m410 = "0410422000000a80800801X123123595912345610101020202099TERMINAL484001X";

            var m = factory.ParseMessage(m200.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
            Assert.Equal("TERMINAL", m.GetObjectValue(41));
            Assert.Equal("484", m.GetObjectValue(49));

            m = factory.ParseMessage(m210.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
            Assert.Equal("TERMINAL", m.GetObjectValue(41));
            Assert.Equal("484", m.GetObjectValue(49));
            Assert.Equal("99", m.GetObjectValue(39));

            m = factory.ParseMessage(m400.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
            Assert.Equal("TERMINAL", m.GetObjectValue(41));
            Assert.Equal("484", m.GetObjectValue(49));
            Assert.Equal("X", m.GetObjectValue(62));

            m = factory.ParseMessage(m410.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
            Assert.Equal("TERMINAL", m.GetObjectValue(41));
            Assert.Equal("484", m.GetObjectValue(49));
            Assert.Equal("99", m.GetObjectValue(39));
            Assert.Equal("X", m.GetObjectValue(61));
        }

        // ──────────────────────────────────────────────────────────────────
        //  Headers
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestHeaders()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithHeader(0x0200, "ISO015000050")
                .WithHeader(0x0800, "ISO015000015")
                .WithHeaderRef(0x0810, 0x0800)
                .WithTemplate(0x0200, t => t
                    .Field(3, IsoType.NUMERIC, "650000", 6))
                .Build();

            Assert.Equal("ISO015000050", factory.GetIsoHeader(0x0200));
            Assert.Equal("ISO015000015", factory.GetIsoHeader(0x0800));
            Assert.Equal(factory.GetIsoHeader(0x0800), factory.GetIsoHeader(0x0810));

            var m = factory.NewMessage(0x0200);
            Assert.NotNull(m);
            Assert.Equal("ISO015000050", m.IsoHeader);
        }

        [Fact]
        public void TestBinaryHeader()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithBinaryHeader(0x0200, new byte[] { 0xFF, 0xFE, 0xFD })
                .WithTemplate(0x0200, t => t
                    .Field(3, IsoType.NUMERIC, "000000", 6))
                .Build();

            var header = factory.GetBinaryIsoHeader(0x0200);
            Assert.NotNull(header);
            Assert.Equal(3, header.Length);
        }

        // ──────────────────────────────────────────────────────────────────
        //  Factory properties
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestFactoryProperties()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithEncoding(Encoding.UTF8)
                .WithBinaryMessages()
                .WithBinaryBitmap()
                .WithEnforceSecondBitmap()
                .WithEtx(0x03)
                .WithIgnoreLast()
                .WithAssignDate()
                .Build();

            Assert.True(factory.UseBinaryMessages);
            Assert.True(factory.UseBinaryBitmap);
            Assert.True(factory.EnforceSecondBitmap);
            Assert.Equal(0x03, factory.Etx);
            Assert.True(factory.IgnoreLast);
            Assert.True(factory.AssignDate);
            Assert.Equal(Encoding.UTF8, factory.Encoding);
        }

        [Fact]
        public void TestForceStringEncoding()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithEncoding(Encoding.UTF8)
                .WithForceStringEncoding()
                .Build();

            Assert.True(factory.ForceStringEncoding);
        }

        [Fact]
        public void TestRadix()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithRadix(16)
                .Build();

            Assert.Equal(16, factory.Radix);
        }

        // ──────────────────────────────────────────────────────────────────
        //  Composite field in template
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestCompositeFieldTemplate()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithTemplate(0x0100, t => t
                    .CompositeField(10, IsoType.LLLVAR, cf => cf
                        .SubField(IsoType.ALPHA, "abcde", 5)
                        .SubField(IsoType.LLVAR, "llvar")
                        .SubField(IsoType.NUMERIC, "12345", 5)
                        .SubField(IsoType.ALPHA, "X", 1)))
                .Build();

            var m = factory.NewMessage(0x0100);
            Assert.NotNull(m);
            Assert.True(m.HasField(10));
            var f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal("abcde", f.GetObjectValue(0));
            Assert.Equal("llvar", f.GetObjectValue(1));
            Assert.Equal("12345", f.GetObjectValue(2));
            Assert.Equal("X", f.GetObjectValue(3));
        }

        // ──────────────────────────────────────────────────────────────────
        //  Composite field in parse map
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestCompositeFieldParseMap()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithParseMap(0x0100, p => p
                    .CompositeField(10, IsoType.LLLVAR, cf => cf
                        .SubParser(IsoType.ALPHA, 5)
                        .SubParser(IsoType.LLVAR)
                        .SubParser(IsoType.NUMERIC, 5)
                        .SubParser(IsoType.ALPHA, 1)))
                .Build();

            var msg = "01000040000000000000016one  03two12345.";
            var m = factory.ParseMessage(msg.GetSignedBytes(), 0);
            Assert.NotNull(m);
            var f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
            Assert.Equal("one  ", f.GetObjectValue(0));
            Assert.Equal("two", f.GetObjectValue(1));
            Assert.Equal("12345", f.GetObjectValue(2));
            Assert.Equal(".", f.GetObjectValue(3));
        }

        // ──────────────────────────────────────────────────────────────────
        //  Full round-trip: create message -> write -> parse
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestRoundTrip()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithEncoding(Encoding.UTF8)
                .WithTemplate(0x0200, t => t
                    .Field(3, IsoType.NUMERIC, "000000", 6)
                    .Field(11, IsoType.NUMERIC, "000001", 6)
                    .Field(41, IsoType.ALPHA, "TERMINAL", 8)
                    .Field(49, IsoType.ALPHA, "484", 3))
                .WithParseMap(0x0200, p => p
                    .Field(3, IsoType.NUMERIC, 6)
                    .Field(11, IsoType.NUMERIC, 6)
                    .Field(41, IsoType.ALPHA, 8)
                    .Field(49, IsoType.ALPHA, 3))
                .Build();

            var original = factory.NewMessage(0x0200);
            Assert.NotNull(original);

            // Write to byte buffer
            var data = original.WriteData();
            Assert.NotNull(data);

            // Parse it back
            var parsed = factory.ParseMessage(data, 0);
            Assert.NotNull(parsed);
            Assert.Equal(0x0200, parsed.Type);
            Assert.Equal("000000", parsed.GetObjectValue(3));
            Assert.Equal("000001", parsed.GetObjectValue(11));
            Assert.Equal("TERMINAL", parsed.GetObjectValue(41));
            Assert.Equal("484", parsed.GetObjectValue(49));
        }

        // ──────────────────────────────────────────────────────────────────
        //  Create response
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestCreateResponse()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>()
                .WithTemplate(0x0200, t => t
                    .Field(3, IsoType.NUMERIC, "000000", 6)
                    .Field(11, IsoType.NUMERIC, "000001", 6))
                .WithTemplate(0x0210, t => t
                    .Field(39, IsoType.ALPHA, "00", 2))
                .Build();

            var request = factory.NewMessage(0x0200);
            Assert.NotNull(request);

            var response = factory.CreateResponse(request);
            Assert.NotNull(response);
            Assert.Equal(0x0210, response.Type);
            // Fields copied from request
            Assert.True(response.HasField(3));
            Assert.True(response.HasField(11));
        }

        // ──────────────────────────────────────────────────────────────────
        //  Equivalent to config.xml: full programmatic configuration
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestFullProgrammaticConfigEquivalentToXml()
        {
            // This replicates the core parts of config.xml programmatically
            var factory = new MessageFactoryBuilder<IsoMessage>()
                // Headers
                .WithHeader(0x0200, "ISO015000050")
                .WithHeader(0x0210, "ISO015000055")
                .WithHeaderRef(0x0400, 0x0200)
                .WithHeaderRef(0x0410, 0x0210)
                .WithHeader(0x0800, "ISO015000015")
                .WithHeaderRef(0x0810, 0x0800)
                // Templates
                .WithTemplate(0x0200, t => t
                    .Field(3, IsoType.NUMERIC, "650000", 6)
                    .Field(32, IsoType.LLVAR, "456")
                    .Field(35, IsoType.LLVAR, "4591700012340000=")
                    .Field(43, IsoType.ALPHA, "SOLABTEST             TEST-3       DF MX", 40)
                    .Field(48, IsoType.LLLVAR, "Life, the Universe, and Everything|42")
                    .Field(49, IsoType.ALPHA, "484", 3)
                    .Field(60, IsoType.LLLVAR, "B456PRO1+000")
                    .Field(61, IsoType.LLLVAR,
                        "        1234P vamos a meter más de 90 caracteres en este campo para comprobar si hay algun error en el parseo del mismo. Esta definido como un LLLVAR aqui por lo tanto esto debe caber sin problemas; las guias de parseo de 200 y 210 tienen LLLVAR en campo 61 tambien.")
                    .Field(100, IsoType.LLVAR, "999")
                    .Field(102, IsoType.LLVAR, "ABCD"))
                .WithTemplate(0x0400, t => t
                    .Extends(0x0200)
                    .Field(90, IsoType.ALPHA, "BLA", 42)
                    .Exclude(102))
                // Parse maps
                .WithParseMap(0x0800, p => p
                    .Field(3, IsoType.ALPHA, 6)
                    .Field(12, IsoType.DATE4)
                    .Field(17, IsoType.DATE4))
                .WithParseMap(0x0810, p => p
                    .Extends(0x0800)
                    .Exclude(17)
                    .Field(39, IsoType.ALPHA, 2))
                .Build();

            // Verify headers
            Assert.Equal("ISO015000050", factory.GetIsoHeader(0x0200));
            Assert.Equal("ISO015000055", factory.GetIsoHeader(0x0210));
            Assert.Equal(factory.GetIsoHeader(0x0200), factory.GetIsoHeader(0x0400));
            Assert.Equal(factory.GetIsoHeader(0x0210), factory.GetIsoHeader(0x0410));
            Assert.Equal("ISO015000015", factory.GetIsoHeader(0x0800));
            Assert.Equal(factory.GetIsoHeader(0x0800), factory.GetIsoHeader(0x0810));

            // Verify templates
            var m200 = factory.GetMessageTemplate(0x0200);
            var m400 = factory.GetMessageTemplate(0x0400);
            Assert.NotNull(m200);
            Assert.NotNull(m400);

            // 0x0400 inherits from 0x0200
            for (var i = 2; i < 89; i++)
            {
                var v = m200.GetField(i);
                if (v == null)
                    Assert.False(m400.HasField(i));
                else
                    Assert.True(m400.HasField(i));
            }

            Assert.False(m200.HasField(90));
            Assert.True(m400.HasField(90));
            Assert.True(m200.HasField(102));
            Assert.False(m400.HasField(102));

            // Verify parsing
            var s800 = "0800201080000000000012345611251125";
            var s810 = "08102010000002000000123456112500";

            var m = factory.ParseMessage(s800.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.True(m.HasField(17));
            Assert.False(m.HasField(39));

            m = factory.ParseMessage(s810.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.False(m.HasField(17));
            Assert.True(m.HasField(39));
        }

        // ──────────────────────────────────────────────────────────────────
        //  Error cases
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestExtendsNonexistentTemplateThrows()
        {
            var builder = new MessageFactoryBuilder<IsoMessage>()
                .WithTemplate(0x0210, t => t
                    .Extends(0x0200)
                    .Field(39, IsoType.ALPHA, "00", 2));

            Assert.Throws<System.ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void TestExtendsNonexistentParseMapThrows()
        {
            var builder = new MessageFactoryBuilder<IsoMessage>()
                .WithParseMap(0x0810, p => p
                    .Extends(0x0800)
                    .Field(39, IsoType.ALPHA, 2));

            Assert.Throws<System.ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void TestInvalidFieldNumberThrows()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                new MessageFactoryBuilder<IsoMessage>()
                    .WithTemplate(0x0200, t => t
                        .Field(1, IsoType.ALPHA, "X", 1));
            });

            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
            {
                new MessageFactoryBuilder<IsoMessage>()
                    .WithTemplate(0x0200, t => t
                        .Field(129, IsoType.ALPHA, "X", 1));
            });
        }

        [Fact]
        public void TestFixedLengthTypeWithoutLengthThrows()
        {
            Assert.Throws<System.ArgumentException>(() =>
            {
                new MessageFactoryBuilder<IsoMessage>()
                    .WithTemplate(0x0200, t => t
                        .Field(3, IsoType.NUMERIC, "000000"));
            });
        }

        // ──────────────────────────────────────────────────────────────────
        //  Empty factory
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public void TestEmptyBuilder()
        {
            var factory = new MessageFactoryBuilder<IsoMessage>().Build();
            Assert.NotNull(factory);
        }
    }
}
