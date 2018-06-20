using NetCore8583.Util;
using Xunit;

namespace NetCore8583.Test
{
    public class TestLlll
    {
        private MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();

        public TestLlll()
        {
            mfact.SetConfigPath(@"/Resources/issue36.xml");
            mfact.AssignDate = false;
        }

        [Fact]
        public void TestTemplate()
        {
            IsoMessage m = mfact.NewMessage(0x100);
            Assert.Equal("010060000000000000000001X0002FF", m.DebugString());
            m.Binary = true;
            Assert.Equal(new sbyte[]
            {
                1, 0, (sbyte) 0x60, 0, 0, 0, 0, 0, 0, 0,
                0, 1, (sbyte) 'X', 0, 1, unchecked((sbyte) 0xff)
            }, m.WriteData());
        }

        [Fact]
        public void TestNewMessage()
        {
            IsoMessage m = mfact.NewMessage(0x200);
            m.SetValue(2, "Variable length text", IsoType.LLLLVAR, 0);
            m.SetValue(3, "FFFF", IsoType.LLLLBIN, 0);
            Assert.Equal("020060000000000000000020Variable length text0004FFFF", m.DebugString());
            m.Binary = (true);
            m.SetValue(2, "XX", IsoType.LLLLVAR, 0);
            m.SetValue(3, new sbyte[] {unchecked((sbyte) 0xff)}, IsoType.LLLLBIN, 0);
            Assert.Equal(new sbyte[]
            {
                2, 0, (sbyte) 0x60, 0, 0, 0, 0, 0, 0, 0,
                0, 2, (sbyte) 'X', (sbyte) 'X', 0, 1, unchecked((sbyte) 0xff)
            }, m.WriteData());
        }

        [Fact]
        public void TestParsing()
        {
            IsoMessage m = mfact.ParseMessage("010060000000000000000001X0002FF".GetSignedbytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal(new sbyte[] {unchecked((sbyte) 0xff)}, (sbyte[]) m.GetObjectValue(3));
            mfact.UseBinaryMessages = (true);
            m = mfact.ParseMessage(new byte[]
            {
                1, 0, (byte) 0x60, 0, 0, 0, 0, 0, 0, 0,
                0, 2, (byte) 'X', (byte) 'X', 0, 1, (byte) 0xff
            }.ToSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("XX", m.GetObjectValue(2));
            Assert.Equal(new sbyte[] {unchecked((sbyte) 0xff)}, (sbyte[]) m.GetObjectValue(3));
        }
    }
}