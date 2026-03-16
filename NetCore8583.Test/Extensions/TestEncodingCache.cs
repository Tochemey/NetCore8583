using System.Text;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestEncodingCache
    {
        [Fact]
        public void Utf8IsUtf8Encoding()
        {
            Assert.NotNull(EncodingCache.Utf8);
            Assert.Equal(Encoding.UTF8, EncodingCache.Utf8);
        }

        [Fact]
        public void DefaultIsDefaultEncoding()
        {
            Assert.NotNull(EncodingCache.Default);
            Assert.Equal(Encoding.Default, EncodingCache.Default);
        }

        [Fact]
        public void AsciiIsAsciiEncoding()
        {
            Assert.NotNull(EncodingCache.Ascii);
            Assert.Equal(Encoding.ASCII, EncodingCache.Ascii);
        }

        [Fact]
        public void UnicodeIsUnicodeEncoding()
        {
            Assert.NotNull(EncodingCache.Unicode);
            Assert.Equal(Encoding.Unicode, EncodingCache.Unicode);
        }

        [Fact]
        public void Utf32IsUtf32Encoding()
        {
            Assert.NotNull(EncodingCache.Utf32);
            Assert.Equal(Encoding.UTF32, EncodingCache.Utf32);
        }
    }
}
