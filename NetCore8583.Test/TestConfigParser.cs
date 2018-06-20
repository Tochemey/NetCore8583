using NetCore8583.Codecs;
using NetCore8583.Util;
using Xunit;

namespace NetCore8583.Test
{
    public class TestConfigParser
    {
        private MessageFactory<IsoMessage> Config(string path)
        {
            MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();
            mfact.SetConfigPath(path);
            return mfact;
        }

        [Fact]
        public void TestParser()
        {
            string configXml = @"/Resources/config.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);

            //Headers
            Assert.NotNull(mfact.GetIsoHeader(0x800));
            Assert.NotNull(mfact.GetIsoHeader(0x810));
            Assert.Equal(mfact.GetIsoHeader(0x800), mfact.GetIsoHeader(0x810));

            //Templates
            IsoMessage m200 = mfact.GetMessageTemplate(0x200);
            Assert.NotNull(m200);
            IsoMessage m400 = mfact.GetMessageTemplate(0x400);
            Assert.NotNull(m400);

            for (int i = 2; i < 89; i++)
            {
                IsoValue v = m200.GetField(i);
                if (v == null)
                {
                    Assert.False(m400.HasField(i));
                }
                else
                {
                    Assert.True(m400.HasField(i));
                    Assert.Equal(v, m400.GetField(i));
                }
            }

            Assert.False(m200.HasField(90));
            Assert.True(m400.HasField(90));
            Assert.True(m200.HasField(102));
            Assert.False(m400.HasField(102));

            //Parsing guides
            string s800 = "0800201080000000000012345611251125";
            string s810 = "08102010000002000000123456112500";
            IsoMessage m = mfact.ParseMessage(s800.GetSignedbytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.True(m.HasField(17));
            Assert.False(m.HasField(39));
            m = mfact.ParseMessage(s810.GetSignedbytes(),
                0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.False(m.HasField(17));
            Assert.True(m.HasField(39));
        }

        [Fact]
        public void TestSimpleCompositeParsers()
        {
            string configXml = @"/Resources/composites.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);
            IsoMessage m = mfact.ParseMessage("01000040000000000000016one  03two12345.".GetSignedbytes(), 0);
            Assert.NotNull(m);
            CompositeField f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
            Assert.Equal("one  ", f.GetObjectValue(0));
            Assert.Equal("two", f.GetObjectValue(1));
            Assert.Equal("12345", f.GetObjectValue(2));
            Assert.Equal(".", f.GetObjectValue(3));

            m = mfact.ParseMessage("01000040000000000000018ALPHA05LLVAR12345X".GetSignedbytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(10));
            f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f.GetField(0));
            Assert.NotNull(f.GetField(1));
            Assert.NotNull(f.GetField(2));
            Assert.NotNull(f.GetField(3));
            Assert.Null(f.GetField(4));
            Assert.Equal("ALPHA", f.GetObjectValue(0));
            Assert.Equal("LLVAR", f.GetObjectValue(1));
            Assert.Equal("12345", f.GetObjectValue(2));
            Assert.Equal("X", f.GetObjectValue(3));
        }

        [Fact]
        public void TestNestedCompositeParser()
        {
            string configXml = @"/Resources/composites.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);
            IsoMessage m = mfact.ParseMessage("01010040000000000000019ALPHA11F1F205F03F4X".GetSignedbytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(10));
            CompositeField f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f.GetField(0));
            Assert.NotNull(f.GetField(1));
            Assert.NotNull(f.GetField(2));
            Assert.Null(f.GetField(3));
            Assert.Equal("ALPHA", f.GetObjectValue(0));
            Assert.Equal("X", f.GetObjectValue(2));
            f = (CompositeField) f.GetObjectValue(1);
            Assert.Equal("F1", f.GetObjectValue(0));
            Assert.Equal("F2", f.GetObjectValue(1));
            f = (CompositeField) f.GetObjectValue(2);
            Assert.Equal("F03", f.GetObjectValue(0));
            Assert.Equal("F4", f.GetObjectValue(1));
        }

        [Fact]
        public void TestSimpleCompositeTemplate()
        {
            string configXml = @"/Resources/composites.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);
            IsoMessage m = mfact.NewMessage(0x100);
            //Simple composite
            Assert.NotNull(m);
            Assert.False(m.HasField(1));
            Assert.False(m.HasField(2));
            Assert.False(m.HasField(3));
            Assert.False(m.HasField(4));
            CompositeField f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal("abcde", f.GetObjectValue(0));
            Assert.Equal("llvar", f.GetObjectValue(1));
            Assert.Equal("12345", f.GetObjectValue(2));
            Assert.Equal("X", f.GetObjectValue(3));
            Assert.False(m.HasField(4));
        }

        private void NestedCompositeTemplate(int type, int fnum)
        {
            string configXml = @"/Resources/composites.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);
            IsoMessage m = mfact.NewMessage(type);
            Assert.NotNull(m);
            Assert.False(m.HasField(1));
            Assert.False(m.HasField(2));
            Assert.False(m.HasField(3));
            Assert.False(m.HasField(4));
            CompositeField f = (CompositeField) m.GetObjectValue(fnum);
            Assert.Equal("fghij", f.GetObjectValue(0));
            Assert.Equal("67890", f.GetObjectValue(2));
            Assert.Equal("Y", f.GetObjectValue(3));
            f = (CompositeField) f.GetObjectValue(1);
            Assert.Equal("KL", f.GetObjectValue(0));
            Assert.Equal("mn", f.GetObjectValue(1));
            f = (CompositeField) f.GetObjectValue(2);
            Assert.Equal("123", f.GetObjectValue(0));
            Assert.Equal("45", f.GetObjectValue(1));
        }

        [Fact]
        public void TestNestedCompositeTemplate()
        {
            NestedCompositeTemplate(0x101, 10);
        }

        [Fact]
        public void TestNestedCompositeFromExtendedTemplate()
        {
            NestedCompositeTemplate(0x102, 10);
            NestedCompositeTemplate(0x102, 12);
        }

        [Fact]
        public void TestMultilevelExtendParseGuides()
        {
            string configXml = @"/Resources/issue34.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);

            //Parse a 200
            string m200 = "0200422000000880800001X1231235959123456101010202020TERMINAL484";
            string m210 = "0210422000000A80800001X123123595912345610101020202099TERMINAL484";
            string m400 = "0400422000000880800401X1231235959123456101010202020TERMINAL484001X";
            string m410 = "0410422000000a80800801X123123595912345610101020202099TERMINAL484001X";

            IsoMessage m = mfact.ParseMessage(m200.GetSignedbytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
            Assert.Equal("TERMINAL", m.GetObjectValue(41));
            Assert.Equal("484", m.GetObjectValue(49));
            m = mfact.ParseMessage(m210.GetSignedbytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
            Assert.Equal("TERMINAL", m.GetObjectValue(41));
            Assert.Equal("484", m.GetObjectValue(49));
            Assert.Equal("99", m.GetObjectValue(39));
            m = mfact.ParseMessage(m400.GetSignedbytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
            Assert.Equal("TERMINAL", m.GetObjectValue(41));
            Assert.Equal("484", m.GetObjectValue(49));
            Assert.Equal("X", m.GetObjectValue(62));
            m = mfact.ParseMessage(m410.GetSignedbytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
            Assert.Equal("TERMINAL", m.GetObjectValue(41));
            Assert.Equal("484", m.GetObjectValue(49));
            Assert.Equal("99", m.GetObjectValue(39));
            Assert.Equal("X", m.GetObjectValue(61));
        }

        [Fact]
        public void TestExtendCompositeWithSameField()
        {
            string configXml = @"/Resources/issue47.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);

            string m200 = "02001000000000000004000000100000013ABCDEFGHIJKLM";

            IsoMessage isoMessage = mfact.ParseMessage(m200.GetSignedbytes(), 0);

            // check field num 4
            IsoValue field4 = isoMessage.GetField(4);
            Assert.Equal(IsoType.AMOUNT, field4.Type);
            Assert.Equal(IsoType.AMOUNT.Length(), field4.Length);

            // check nested field num 4 from composite field 62
            CompositeField compositeField62 = (CompositeField) isoMessage.GetField(62).Value;
            IsoValue nestedField4 = compositeField62.GetField(0); // first in list
            Assert.Equal(IsoType.ALPHA, nestedField4.Type);
            Assert.Equal(13, nestedField4.Length);
        }

        [Fact]
        public void TestEmptyFields()
        {
            string configXml = @"/Resources/issue64.xml";
            MessageFactory<IsoMessage> mfact = Config(configXml);
            IsoMessage msg = mfact.NewMessage(0x200);
            Assert.Equal("", msg.GetObjectValue(3));
        }
    }
}