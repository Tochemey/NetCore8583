using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetCore8583.Util;
using Xunit;

namespace NetCore8583.Test
{
    public class TestIsoMessage
    {
        public TestIsoMessage()
        {
            mf = new MessageFactory<IsoMessage>
            {
                Encoding = Encoding.UTF8
            };
            mf.SetCustomField(48,
                new CustomField48());
            mf.SetConfigPath(@"/Resources/config.xml");
        }

        private readonly MessageFactory<IsoMessage> mf;

        private void TestFields(IsoMessage m,
            List<int> fields)
        {
            for (var i = 2; i < 128; i++)
                if (fields.Contains(i)) Assert.True(m.HasField(i));
                else Assert.False(m.HasField(i));
        }

        [Fact]
        public void TestCreation()
        {
            var iso = mf.NewMessage(0x200);
            Assert.Equal(0x200,
                iso.Type);
            Assert.True(iso.HasEveryField(3,
                32,
                35,
                43,
                48,
                49,
                60,
                61,
                100,
                102));
            Assert.Equal(IsoType.NUMERIC,
                iso.GetField(3).Type);
            Assert.Equal("650000",
                iso.GetObjectValue(3));
            Assert.Equal(IsoType.LLVAR,
                iso.GetField(32).Type);
            Assert.Equal(IsoType.LLVAR,
                iso.GetField(35).Type);
            Assert.Equal(IsoType.ALPHA,
                iso.GetField(43).Type);
            Assert.Equal(40,
                ((string) iso.GetObjectValue(43)).Length);
            Assert.Equal(IsoType.LLLVAR,
                iso.GetField(48).Type);
            Assert.True(iso.GetObjectValue(48) is CustomField48);
            Assert.Equal(IsoType.ALPHA,
                iso.GetField(49).Type);
            Assert.Equal(IsoType.LLLVAR,
                iso.GetField(60).Type);
            Assert.Equal(IsoType.LLLVAR,
                iso.GetField(61).Type);
            Assert.Equal(IsoType.LLVAR,
                iso.GetField(100).Type);
            Assert.Equal(IsoType.LLVAR,
                iso.GetField(102).Type);

            for (var i = 4; i < 32; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
            for (var i = 36; i < 43; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
            for (var i = 50; i < 60; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
            for (var i = 62; i < 100; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
            for (var i = 103; i < 128; i++)
                Assert.False(iso.HasField(i),
                    "ISO should not contain " + i);
        }

        [Fact]
        public void TestEncoding()
        {
            var m1 = mf.NewMessage(0x200);
            var buf = m1.WriteData();
            var m2 = mf.ParseMessage(buf,
                mf.GetIsoHeader(0x200).Length);
            Assert.Equal(m2.Type,
                m1.Type);
            for (var i = 2; i < 128; i++)
                //Either both have the field or neither have it
                if (m1.HasField(i) && m2.HasField(i))
                {
                    Assert.Equal(m1.GetField(i).Type,
                        m2.GetField(i).Type);
                    Assert.Equal(m1.GetObjectValue(i),
                        m2.GetObjectValue(i));
                }
                else
                {
                    Assert.False(m1.HasField(i));
                    Assert.False(m2.HasField(i));
                }
        }

        [Fact]
        public void TestParsing()
        {
            var read = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"/Resources/parse1.txt");
            var buf = read.ToInt8();
            var len = mf.GetIsoHeader(0x210).Length;
            var iso = mf.ParseMessage(buf,
                len);
            Assert.Equal(0x210,
                iso.Type);
            var b2 = iso.WriteData();

            //Remove leftover newline and stuff from the original buffer
            var b3 = new sbyte[b2.Length];
            Array.Copy(buf,
                0,
                b3,
                0,
                b3.Length);
            Assert.Equal(b3,
                b2);

            //Test it contains the correct fields
            var fields = new List<int>
            {
                3,
                4,
                7,
                11,
                12,
                13,
                15,
                17,
                32,
                35,
                37,
                38,
                39,
                41,
                43,
                49,
                60,
                61,
                100,
                102,
                126
            };

            TestFields(iso,
                fields);
            //Again, but now with forced encoding
            mf.ForceStringEncoding = true;
            iso = mf.ParseMessage(buf,
                mf.GetIsoHeader(0x210).Length);
            Assert.Equal(0x210,
                iso.Type);
            TestFields(iso,
                fields);
        }

        [Fact]
        public void TestSimpleFieldSetter()
        {
            var iso = mf.NewMessage(0x200);
            var f3 = iso.GetField(3);
            iso.UpdateValue(3, "999999");
            Assert.Equal("999999", iso.GetObjectValue(3));
            var nf3 = iso.GetField(3);
            Assert.NotSame(f3, nf3);
            Assert.Equal(f3.Type, nf3.Type);
            Assert.Equal(f3.Length, nf3.Length);
            Assert.Same(f3.Encoder, nf3.Encoder);
            Assert.Throws<ArgumentException>(() => iso.UpdateValue(4, "INVALID!"))
                ;
        }

        [Fact]
        public void TestTemplating()
        {
            var iso1 = mf.NewMessage(0x200);
            var iso2 = mf.NewMessage(0x200);
            Assert.NotSame(iso1, iso2);
            Assert.Equal(iso1.GetObjectValue(3), iso2.GetObjectValue(3));
            Assert.NotSame(iso1.GetField(3), iso2.GetField(3));
            Assert.NotSame(iso1.GetField(48), iso2.GetField(48));
            var cf48_1 = (CustomField48) iso1.GetObjectValue(48);
            var origv = cf48_1.V2;
            cf48_1.V2 = origv + 1000;
            var cf48_2 = (CustomField48) iso2.GetObjectValue(48);
            Assert.Same(cf48_1, cf48_2);
            Assert.Equal(cf48_2.V2, origv + 1000);
        }
    }
}