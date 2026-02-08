using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class TestEncoding
    {
        [Fact]
        public void WindowsToUtf8()
        {
            var data = "05ácido";
            var encoding = Encoding.UTF8;
            var buf = data.GetSignedBytes(encoding);
            var parser = new LlvarParseInfo
            {
                Encoding = Encoding.Default
            };

            var field = parser.Parse(1, buf, 0, null);
            Assert.Equal(field.Value, data.Substring(2));
            parser.Encoding = encoding;
            field = parser.Parse(1, buf, 0, null);
            Assert.Equal(data.Substring(2), field.Value);
        }
    }
}