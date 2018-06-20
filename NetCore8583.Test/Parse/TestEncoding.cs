using System.Text;
using NetCore8583.Parse;
using NetCore8583.Util;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class TestEncoding
    {
        [Fact]
        public void WindowsToUtf8()
        {
            string data = "05ácido";
            Encoding encoding = Encoding.UTF8;
            sbyte[] buf = data.GetSignedbytes(encoding);
            LlvarParseInfo parser = new LlvarParseInfo
            {
                Encoding = Encoding.Default
            };

            IsoValue field = parser.Parse(1, buf, 0, null);
            Assert.Equal(field.Value, data.Substring(2));
            parser.Encoding = encoding;
            field = parser.Parse(1, buf, 0, null);
            Assert.Equal(data.Substring(2), field.Value);
        }
    }
}