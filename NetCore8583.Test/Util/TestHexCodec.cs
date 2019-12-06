using System;
using NetCore8583.Util;
using Xunit;

namespace NetCore8583.Test.Util
{
    public class TestHexCodec
    {
        public void EncodeDecode(string hex)
        {
            var buf = HexCodec.HexDecode(hex);
            Assert.Equal(hex.Length / 2 + hex.Length % 2,
                buf.Length);
            var reenc = HexCodec.HexEncode(buf,
                0,
                buf.Length);
            if (reenc.StartsWith("0",
                    StringComparison.Ordinal) && !hex.StartsWith("0",
                    StringComparison.Ordinal))
                Assert.Equal(reenc.Substring(1),
                    hex);
            else
                Assert.Equal(hex,
                    reenc);
        }

        [Fact]
        public void TestCodec()
        {
            var buf = HexCodec.HexDecode("A");
            Assert.Equal(0x0a,
                buf[0]);
            EncodeDecode("A");
            EncodeDecode("0123456789ABCDEF");
            buf = HexCodec.HexDecode("0123456789ABCDEF");
            Assert.Equal(1,
                buf[0]);
            Assert.Equal(0x23,
                buf[1]);
            Assert.Equal(0x45,
                buf[2]);
            Assert.Equal(0x67,
                buf[3]);
            Assert.Equal(0x89,
                buf[4] & 0xff);
            Assert.Equal(0xab,
                buf[5] & 0xff);
            Assert.Equal(0xcd,
                buf[6] & 0xff);
            Assert.Equal(0xef,
                buf[7] & 0xff);
            buf = HexCodec.HexDecode("ABC");
            Assert.Equal(0x0a,
                buf[0] & 0xff);
            Assert.Equal(0xbc,
                buf[1] & 0xff);
            EncodeDecode("ABC");
        }

        [Fact]
        public void TestPartial()
        {
            Assert.Equal("FF01", HexCodec.HexEncode(new byte[] {0, 0xff, 1, 2, 3, 4}.ToSignedBytes(),
                1, 2));
        }
    }
}