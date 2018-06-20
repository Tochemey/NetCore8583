using System.Text;
using NetCore8583.Codecs;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class TestComposites
    {
        private string textData = "One  03Two00999X";

        sbyte[] binaryData = new sbyte[]
        {
            (sbyte) 'O', (sbyte) 'n', (sbyte) 'e', (sbyte) ' ', (sbyte) ' ', 3, (sbyte) 'T', (sbyte) 'w', (sbyte) 'o',
            0, 9, unchecked((sbyte) 0x99), (sbyte) 'X'
        };

        [Fact]
        public void TestEncodeText()
        {
            CompositeField f = new CompositeField();
            f.AddValue(new IsoValue(IsoType.ALPHA, "One", 5));
            f.Values[0].Encoding = Encoding.UTF8;
            Assert.Equal("One  ", f.EncodeField(f));
            f.AddValue("Two", null, IsoType.LLVAR, 0);
            f.Values[1].Encoding = Encoding.UTF8;
            Assert.Equal("One  03Two", f.EncodeField(f));
            f.AddValue(999, null, IsoType.NUMERIC, 5);
            f.Values[2].Encoding = Encoding.UTF8;
            Assert.Equal("One  03Two00999", f.EncodeField(f));
            f.AddValue("X", null, IsoType.ALPHA, 1);
            Assert.Equal(textData, f.EncodeField(f));
        }

        [Fact]
        public void TestEncodeBinary()
        {
            CompositeField f = new CompositeField()
                .AddValue(new IsoValue(IsoType.ALPHA, "One", 5));
            Assert.Equal(new sbyte[] {(sbyte) 'O', (sbyte) 'n', (sbyte) 'e', 32, 32}, f.EncodeBinaryField(f));
            f.AddValue(new IsoValue(IsoType.LLVAR, "Two"));
            Assert.Equal(
                new sbyte[]
                {
                    (sbyte) 'O', (sbyte) 'n', (sbyte) 'e', (sbyte) ' ', (sbyte) ' ', 3, (sbyte) 'T', (sbyte) 'w',
                    (sbyte) 'o'
                },
                f.EncodeBinaryField(f));
            f.AddValue(new IsoValue(IsoType.NUMERIC, 999L, 5));
            f.AddValue(new IsoValue(IsoType.ALPHA, "X", 1));
            Assert.Equal(binaryData, f.EncodeBinaryField(f));
        }

        [Fact]
        public void TestDecodeText()
        {
            CompositeField dec = new CompositeField()
                .AddParser(new AlphaParseInfo(5))
                .AddParser(new LlvarParseInfo())
                .AddParser(new NumericParseInfo(5))
                .AddParser(new AlphaParseInfo(1));

            CompositeField f = (CompositeField) dec.DecodeField(textData);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
            Assert.Equal("One  ", f.Values[0].Value);
            Assert.Equal("Two", f.Values[1].Value);
            Assert.Equal("00999", f.Values[2].Value);
            Assert.Equal("X", f.Values[3].Value);
        }

        [Fact]
        public void TestDecodeBinary()
        {
            CompositeField dec = new CompositeField()
                .AddParser(new AlphaParseInfo(5))
                .AddParser(new LlvarParseInfo())
                .AddParser(new NumericParseInfo(5))
                .AddParser(new AlphaParseInfo(1));

            CompositeField f = (CompositeField) dec.DecodeBinaryField(binaryData, 0, binaryData.Length);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
            Assert.Equal("One  ", f.Values[0].Value);
            Assert.Equal("Two", f.Values[1].Value);
            Assert.Equal(999L, f.Values[2].Value);
            Assert.Equal("X", f.Values[3].Value);
        }

        [Fact]
        public void TestDecodeBinaryWithOffset()
        {
            CompositeField dec = new CompositeField()
                .AddParser(new LlvarParseInfo())
                .AddParser(new NumericParseInfo(5))
                .AddParser(new AlphaParseInfo(1));
            int offset = 5;
            CompositeField f = (CompositeField) dec.DecodeBinaryField(binaryData, offset, binaryData.Length - offset);
            Assert.NotNull(f);
            Assert.Equal(3, f.Values.Count);
            Assert.Equal("Two", f.Values[0].Value);
            Assert.Equal(999L, f.Values[1].Value);
            Assert.Equal("X", f.Values[2].Value);
        }
    }
}