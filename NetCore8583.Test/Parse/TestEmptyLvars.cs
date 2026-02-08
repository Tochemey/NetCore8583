using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class TestEmptyLvars
    {
        public TestEmptyLvars()
        {
            var issue38xml = @"/Resources/issue38.xml";
            txtfact.SetConfigPath(issue38xml);
            binfact.UseBinaryMessages = true;
            binfact.SetConfigPath(issue38xml);
        }

        private static readonly MessageFactory<IsoMessage> txtfact = new MessageFactory<IsoMessage>();
        private static readonly MessageFactory<IsoMessage> binfact = new MessageFactory<IsoMessage>();

        private void CheckString(sbyte[] txt, sbyte[] bin, int field)
        {
            var t = txtfact.ParseMessage(txt, 0);
            var b = binfact.ParseMessage(bin, 0);
            Assert.True(t.HasField(field));
            Assert.True(b.HasField(field));
            var value = (string) t.GetObjectValue(field);
            var valueb = (string) b.GetObjectValue(field);
            Assert.True(value.IsEmpty());
            Assert.True(valueb.IsEmpty());
        }

        private void CheckBin(sbyte[] txt,
            sbyte[] bin,
            int field)
        {
            var t = txtfact.ParseMessage(txt, 0);
            var b = binfact.ParseMessage(bin, 0);
            Assert.True(t.HasField(field));
            Assert.True(b.HasField(field));
            Assert.Empty((sbyte[]) t.GetObjectValue(field));
            Assert.Empty((sbyte[]) b.GetObjectValue(field));
        }

        [Fact]
        public void TestEmptyLlbin()
        {
            var t = txtfact.NewMessage(0x100);
            var b = binfact.NewMessage(0x100);
            t.SetValue(5, new sbyte[0], IsoType.LLBIN, 0);
            b.SetValue(5, new sbyte[0], IsoType.LLBIN, 0);
            CheckBin(t.WriteData(), b.WriteData(), 5);
        }

        [Fact]
        public void TestEmptyLllbin()
        {
            var t = txtfact.NewMessage(0x100);
            var b = binfact.NewMessage(0x100);
            t.SetValue(6, new sbyte[0], IsoType.LLLBIN, 0);
            b.SetValue(6, new sbyte[0], IsoType.LLLBIN, 0);
            CheckBin(t.WriteData(), b.WriteData(), 6);
        }

        [Fact]
        public void TestEmptyLlllbin()
        {
            var t = txtfact.NewMessage(0x100);
            var b = binfact.NewMessage(0x100);
            t.SetValue(7, new sbyte[0], IsoType.LLLLBIN, 0);
            b.SetValue(7, new sbyte[0], IsoType.LLLLBIN, 0);
            CheckBin(t.WriteData(), b.WriteData(), 7);
        }

        [Fact]
        public void TestEmptyLlllvar()
        {
            var t = txtfact.NewMessage(0x100);
            var b = binfact.NewMessage(0x100);
            t.SetValue(4, "", IsoType.LLLLVAR, 0);
            b.SetValue(4, "", IsoType.LLLLVAR, 0);
            CheckString(t.WriteData(), b.WriteData(), 4);
        }

        [Fact]
        public void TestEmptyLllvar()
        {
            var t = txtfact.NewMessage(0x100);
            var b = binfact.NewMessage(0x100);
            t.SetValue(3, "", IsoType.LLLVAR, 0);
            b.SetValue(3, "", IsoType.LLLVAR, 0);
            CheckString(t.WriteData(), b.WriteData(), 3);
        }

        [Fact]
        public void TestEmptyLlvar()
        {
            var t = txtfact.NewMessage(0x100);
            var b = binfact.NewMessage(0x100);
            t.SetValue(2, "", IsoType.LLVAR, 0);
            b.SetValue(2, "", IsoType.LLVAR, 0);
            CheckString(t.WriteData(), b.WriteData(), 2);
        }
    }
}