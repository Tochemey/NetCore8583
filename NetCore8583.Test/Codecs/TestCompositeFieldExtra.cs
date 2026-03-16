using NetCore8583.Codecs;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Codecs
{
    /// <summary>
    /// Additional CompositeField tests not covered by TestComposites.cs.
    /// Targets GetField bounds, GetParsers, ToString, and AddValue with encoder.
    /// </summary>
    public class TestCompositeFieldExtra
    {
        [Fact]
        public void GetField_NegativeIndex_ReturnsNull()
        {
            var cf = new CompositeField();
            cf.AddValue(new IsoValue(IsoType.ALPHA, "test", 4));
            Assert.Null(cf.GetField(-1));
        }

        [Fact]
        public void GetField_IndexBeyondCount_ReturnsNull()
        {
            var cf = new CompositeField();
            cf.AddValue(new IsoValue(IsoType.ALPHA, "test", 4));
            Assert.Null(cf.GetField(5));
        }

        [Fact]
        public void GetParsers_BeforeAddingParsers_ReturnsNull()
        {
            var cf = new CompositeField();
            Assert.Null(cf.GetParsers());
        }

        [Fact]
        public void GetParsers_AfterAddingParser_ReturnsList()
        {
            var cf = new CompositeField();
            cf.AddParser(new AlphaParseInfo(5));
            Assert.Single(cf.GetParsers());
        }

        [Fact]
        public void GetParsers_MultipleAdded_ReturnsAll()
        {
            var cf = new CompositeField();
            cf.AddParser(new AlphaParseInfo(5));
            cf.AddParser(new LlvarParseInfo());
            Assert.Equal(2, cf.GetParsers().Count);
        }

        [Fact]
        public void ToString_NullValues_ReturnsEmptyBrackets()
        {
            var cf = new CompositeField();
            Assert.Equal("CompositeField[]", cf.ToString());
        }

        [Fact]
        public void ToString_WithSingleValue_ReturnsTypeName()
        {
            var cf = new CompositeField();
            cf.AddValue(new IsoValue(IsoType.ALPHA, "test", 4));
            Assert.Equal("CompositeField[ALPHA]", cf.ToString());
        }

        [Fact]
        public void ToString_WithMultipleValues_ReturnsCommaSeparatedTypes()
        {
            var cf = new CompositeField();
            cf.AddValue(new IsoValue(IsoType.ALPHA, "test", 4));
            cf.AddValue(new IsoValue(IsoType.LLVAR, "var"));
            cf.AddValue(new IsoValue(IsoType.NUMERIC, 123L, 3));
            Assert.Equal("CompositeField[ALPHA,LLVAR,NUMERIC]", cf.ToString());
        }

        [Fact]
        public void AddValue_ObjectMethod_WithNeedsLengthType_SetsLength()
        {
            var cf = new CompositeField();
            cf.AddValue("hello", null, IsoType.ALPHA, 5);
            Assert.Single(cf.Values);
            Assert.Equal(IsoType.ALPHA, cf.GetField(0).Type);
            Assert.Equal(5, cf.GetField(0).Length);
        }

        [Fact]
        public void AddValue_ObjectMethod_WithVariableLengthType_NoLength()
        {
            var cf = new CompositeField();
            cf.AddValue("hello", null, IsoType.LLVAR, 0);
            Assert.Single(cf.Values);
            Assert.Equal(IsoType.LLVAR, cf.GetField(0).Type);
        }

        [Fact]
        public void AddValue_ChainedCalls_AllValuesAdded()
        {
            var cf = new CompositeField();
            cf.AddValue(new IsoValue(IsoType.ALPHA, "a", 1))
              .AddValue(new IsoValue(IsoType.ALPHA, "b", 1))
              .AddValue(new IsoValue(IsoType.ALPHA, "c", 1));
            Assert.Equal(3, cf.Values.Count);
        }
    }
}
