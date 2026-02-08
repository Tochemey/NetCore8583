using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test
{
    public class TestRadix
    {
        private readonly MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();

        public TestRadix()
        {
            mfact.SetConfigPath(@"/Resources/radix.xml");
            mfact.ForceStringEncoding = true;
        }

        [Fact]
        public void TestParseLengthWithRadix10()
        {
            // Given
            var input = "0100" +  // MTI
                        "7000000000000000" + // bitmap
                        "10" + "ABCDEFGHIJ" + // F2 length (10 = 10) + value
                        "26" + "01234567890123456789012345" +  // F3 length (26 = 26) + value
                        "ZZZZZZZZ"; // F4
            
            // When
            IsoMessage m = mfact.ParseMessage(input.GetSignedBytes(), 0);
            
            // Then
            Assert.NotNull(m);
            Assert.Equal("ABCDEFGHIJ", m.GetObjectValue(2));
            Assert.Equal("01234567890123456789012345", HexCodec.HexEncode((sbyte[]) m.GetObjectValue(3), 0, 13));
            Assert.Equal("ZZZZZZZZ", m.GetObjectValue(4));
        }

        [Fact]
        public void TestParseLengthWithRadix16()
        {
            // Given
            mfact.Radix = 16;
            var input = "0100" +  // MTI
                        "7000000000000000" + // bitmap
                        "0A" + "ABCDEFGHIJ" +  // F2 length (0A = 10) + value
                        "1A" + "01234567890123456789012345" +   // F3 length (1A = 26) + value
                        "ZZZZZZZZ"; // F4
            
            // When
            IsoMessage m = mfact.ParseMessage(input.GetSignedBytes(), 0);
            
            // Then
            Assert.NotNull(m);
            Assert.Equal("ABCDEFGHIJ", m.GetObjectValue(2));
            Assert.Equal("01234567890123456789012345", HexCodec.HexEncode((sbyte[]) m.GetObjectValue(3), 0, 13));
            Assert.Equal("ZZZZZZZZ", m.GetObjectValue(4));
        }
    }
}