using System.Text;
using Xunit;

namespace NetCore8583.Test
{
    public class TestHeaders
    {
        public TestHeaders()
        {
            mf = new MessageFactory<IsoMessage>
            {
                Encoding = Encoding.UTF8
            };
            mf.SetConfigPath(@"/Resources/config.xml");
        }

        private readonly MessageFactory<IsoMessage> mf;

        [Fact]
        public void TestBinaryHeader()
        {
            var m = mf.NewMessage(0x280);
            Assert.NotNull(m.BinIsoHeader);
            var buf = m.WriteData();
            Assert.Equal(4 + 4 + 16 + 2, buf.Length);
            for (var i = 0; i < 4; i++) Assert.Equal(buf[i], unchecked((sbyte) 0xff));
            Assert.Equal(buf[4], 0x30);
            Assert.Equal(buf[5], 0x32);
            Assert.Equal(buf[6], 0x38);
            Assert.Equal(buf[7], 0x30);
            //Then parse and check the header is binary 0xffffffff
            m = mf.ParseMessage(buf, 4, true);
            Assert.Null(m.IsoHeader);
            buf = m.BinIsoHeader;
            Assert.NotNull(buf);
            for (var i = 0; i < 4; i++) Assert.Equal(buf[i], unchecked((sbyte) 0xff));
            Assert.Equal(0x280, m.Type);
            Assert.True(m.HasField(3));
        }
    }
}