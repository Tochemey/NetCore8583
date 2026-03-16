using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test
{
    /// <summary>
    /// Tests for issue #15: conditional fields beyond bit 64 when secondary bitmap is absent.
    /// When a parse template defines fields > 64 (e.g. field 125) but a message only contains
    /// fields within the primary bitmap, parsing must not throw.
    /// </summary>
    public class TestIssue15
    {
        // Message type 9800 as ASCII: '9','8','0','0'
        private static readonly byte[] Mti = { 0x39, 0x38, 0x30, 0x30 };

        // Primary-only BCD bitmap: bit 24 (field 24) and bit 31 (field 31) set, bit 1 = 0 (no secondary)
        // Byte 3 (bits 17-24): 0x01 sets bit 24
        // Byte 4 (bits 25-32): 0x02 sets bit 31
        private static readonly byte[] PrimaryBitmap = { 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00 };

        // Primary bitmap with secondary indicator: same as above but bit 1 (MSB of byte 1) = 1
        private static readonly byte[] PrimaryBitmapWithSecondary = { 0x80, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00 };

        // Secondary bitmap: only bit 125 set.
        // Bit 125 is in secondary byte 8 (bits 121-128): value 0x08
        private static readonly byte[] SecondaryBitmapWith125 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08 };

        // Field 24 NUMERIC n3: "089"
        private static readonly byte[] Field24 = { 0x30, 0x38, 0x39 };

        // Field 31 LLVAR: length "02", data "AB"
        private static readonly byte[] Field31 = { 0x30, 0x32, 0x41, 0x42 };

        // Field 125 LLVAR: length "03", data "XYZ"
        private static readonly byte[] Field125 = { 0x30, 0x33, 0x58, 0x59, 0x5A };

        private static sbyte[] BuildMessage(params byte[][] parts)
        {
            var totalLength = 0;
            foreach (var part in parts) totalLength += part.Length;
            var buf = new byte[totalLength];
            var pos = 0;
            foreach (var part in parts)
            {
                part.CopyTo(buf, pos);
                pos += part.Length;
            }
            return buf.ToInt8();
        }

        [Fact]
        public void TestConditionalField125_NoPrimaryBitmapAbsent_ShouldParseWithoutCrash()
        {
            // Field 125 is in the parse template but the message has no secondary bitmap.
            // Parsing must succeed and field 125 must not be present in the result.
            var mf = new MessageFactory<IsoMessage> { UseBinaryBitmap = true };
            ConfigParser.ConfigureFromClasspathConfig(mf, @"/Resources/issue15.xml");

            var buf = BuildMessage(Mti, PrimaryBitmap, Field24, Field31);
            var msg = mf.ParseMessage(buf, 0);

            Assert.NotNull(msg);
            Assert.Equal(0x9800, msg.Type);
            Assert.True(msg.HasField(24), "Field 24 should be present");
            Assert.True(msg.HasField(31), "Field 31 should be present");
            Assert.False(msg.HasField(125), "Field 125 should be absent when secondary bitmap is not present");
            Assert.Equal("089", msg.GetField(24).ToString());
            Assert.Equal("AB", msg.GetField(31).ToString());
        }

        [Fact]
        public void TestConditionalField125_SecondaryBitmapPresent_ShouldParseField125()
        {
            // Field 125 is in the parse template and the message has a secondary bitmap with bit 125 set.
            var mf = new MessageFactory<IsoMessage> { UseBinaryBitmap = true };
            ConfigParser.ConfigureFromClasspathConfig(mf, @"/Resources/issue15.xml");

            var buf = BuildMessage(Mti, PrimaryBitmapWithSecondary, SecondaryBitmapWith125, Field24, Field31, Field125);
            var msg = mf.ParseMessage(buf, 0);

            Assert.NotNull(msg);
            Assert.Equal(0x9800, msg.Type);
            Assert.True(msg.HasField(24), "Field 24 should be present");
            Assert.True(msg.HasField(31), "Field 31 should be present");
            Assert.True(msg.HasField(125), "Field 125 should be present when secondary bitmap is set");
            Assert.Equal("089", msg.GetField(24).ToString());
            Assert.Equal("AB", msg.GetField(31).ToString());
            Assert.Equal("XYZ", msg.GetField(125).ToString());
        }
    }
}
