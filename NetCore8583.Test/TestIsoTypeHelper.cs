using System;
using Xunit;

namespace NetCore8583.Test
{
    public class TestIsoTypeHelper
    {
        // ── NeedsLength ──────────────────────────────────────────────────────────

        [Theory]
        [InlineData(IsoType.ALPHA)]
        [InlineData(IsoType.NUMERIC)]
        [InlineData(IsoType.BINARY)]
        public void NeedsLength_FixedTypes_ReturnsTrue(IsoType t)
        {
            Assert.True(t.NeedsLength());
        }

        [Theory]
        [InlineData(IsoType.LLVAR)]
        [InlineData(IsoType.LLLVAR)]
        [InlineData(IsoType.LLLLVAR)]
        [InlineData(IsoType.LLBIN)]
        [InlineData(IsoType.LLLBIN)]
        [InlineData(IsoType.LLLLBIN)]
        [InlineData(IsoType.DATE10)]
        [InlineData(IsoType.DATE4)]
        [InlineData(IsoType.DATE_EXP)]
        [InlineData(IsoType.DATE12)]
        [InlineData(IsoType.DATE14)]
        [InlineData(IsoType.DATE6)]
        [InlineData(IsoType.TIME)]
        [InlineData(IsoType.AMOUNT)]
        public void NeedsLength_VariableAndDateTypes_ReturnsFalse(IsoType t)
        {
            Assert.False(t.NeedsLength());
        }

        // ── Length ───────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(IsoType.ALPHA, 0)]
        [InlineData(IsoType.BINARY, 0)]
        [InlineData(IsoType.NUMERIC, 0)]
        [InlineData(IsoType.AMOUNT, 12)]
        [InlineData(IsoType.DATE10, 10)]
        [InlineData(IsoType.DATE12, 12)]
        [InlineData(IsoType.DATE14, 14)]
        [InlineData(IsoType.DATE4, 4)]
        [InlineData(IsoType.DATE_EXP, 4)]
        [InlineData(IsoType.TIME, 6)]
        [InlineData(IsoType.DATE6, 6)]
        [InlineData(IsoType.LLVAR, 0)]
        [InlineData(IsoType.LLLVAR, 0)]
        [InlineData(IsoType.LLLLVAR, 0)]
        [InlineData(IsoType.LLBIN, 0)]
        [InlineData(IsoType.LLLBIN, 0)]
        [InlineData(IsoType.LLLLBIN, 0)]
        public void Length_ReturnsExpectedValue(IsoType t, int expected)
        {
            Assert.Equal(expected, t.Length());
        }

        // ── Format(DateTimeOffset) ───────────────────────────────────────────────

        [Fact]
        public void Format_Date6_ReturnsYyMmDd()
        {
            var dt = new DateTimeOffset(2023, 3, 15, 12, 0, 0, TimeSpan.Zero);
            Assert.Equal("230315", IsoType.DATE6.Format(dt));
        }

        [Fact]
        public void Format_DateTypes_NonDateType_Throws()
        {
            var dt = DateTimeOffset.UtcNow;
            Assert.Throws<ArgumentException>(() => IsoType.ALPHA.Format(dt));
            Assert.Throws<ArgumentException>(() => IsoType.NUMERIC.Format(dt));
            Assert.Throws<ArgumentException>(() => IsoType.AMOUNT.Format(dt));
        }

        // ── Format(string, int) ──────────────────────────────────────────────────

        [Fact]
        public void Format_String_Binary_EvenLength_NoChange()
        {
            Assert.Equal("0102", IsoType.BINARY.Format("0102", 4));
        }

        [Fact]
        public void Format_String_Binary_OddLength_PrependZero()
        {
            // "102" is odd → '0' prepended → "0102"
            Assert.Equal("0102", IsoType.BINARY.Format("102", 4));
        }

        [Fact]
        public void Format_String_Binary_ValueLongerThanLength_Truncates()
        {
            Assert.Equal("0102", IsoType.BINARY.Format("01020304", 4));
        }

        [Fact]
        public void Format_String_Binary_NullValue_AllZeros()
        {
            Assert.Equal("0000", IsoType.BINARY.Format(null, 4));
        }

        [Fact]
        public void Format_String_Binary_ValueShorterThanLength_PadsZeros()
        {
            // "01" is even, length 6 → "010000"
            Assert.Equal("010000", IsoType.BINARY.Format("01", 6));
        }

        [Fact]
        public void Format_String_LLBIN_ReturnsValueAsIs()
        {
            Assert.Equal("abc", IsoType.LLBIN.Format("abc", 0));
            Assert.Equal("abc", IsoType.LLLBIN.Format("abc", 0));
            Assert.Equal("abc", IsoType.LLLLBIN.Format("abc", 0));
        }

        [Fact]
        public void Format_String_ALPHA_NullValue_PadsWithSpaces()
        {
            Assert.Equal("   ", IsoType.ALPHA.Format(null, 3));
        }

        [Fact]
        public void Format_String_NUMERIC_ValueTooLarge_Throws()
        {
            Assert.Throws<ArgumentException>(() => IsoType.NUMERIC.Format("12345", 3));
        }

        [Fact]
        public void Format_String_UnsupportedType_Throws()
        {
            Assert.Throws<ArgumentException>(() => IsoType.DATE10.Format("value", 10));
            Assert.Throws<ArgumentException>(() => IsoType.TIME.Format("value", 6));
        }

        // ── Format(long, int) ────────────────────────────────────────────────────

        [Fact]
        public void Format_Long_Numeric_PadsLeft()
        {
            Assert.Equal("000123", IsoType.NUMERIC.Format(123L, 6));
        }

        [Fact]
        public void Format_Long_Numeric_TooLarge_Throws()
        {
            Assert.Throws<ArgumentException>(() => IsoType.NUMERIC.Format(12345L, 3));
        }

        [Fact]
        public void Format_Long_Amount_Returns12Digits()
        {
            Assert.Equal("000001234500", IsoType.AMOUNT.Format(12345L, 0));
        }

        [Fact]
        public void Format_Long_ALPHA_FormatsAsString()
        {
            Assert.Equal("123  ", IsoType.ALPHA.Format(123L, 5));
        }

        [Fact]
        public void Format_Long_LLVAR_ReturnsValueString()
        {
            Assert.Equal("123", IsoType.LLVAR.Format(123L, 0));
        }

        [Fact]
        public void Format_Long_BINARY_ReturnsEmpty()
        {
            Assert.Equal("", IsoType.BINARY.Format(123L, 4));
            Assert.Equal("", IsoType.LLBIN.Format(123L, 0));
            Assert.Equal("", IsoType.LLLBIN.Format(123L, 0));
            Assert.Equal("", IsoType.LLLLBIN.Format(123L, 0));
        }

        [Fact]
        public void Format_Long_UnsupportedType_Throws()
        {
            Assert.Throws<ArgumentException>(() => IsoType.DATE10.Format(123L, 10));
            Assert.Throws<ArgumentException>(() => IsoType.TIME.Format(123L, 6));
        }

        // ── Format(decimal, int) ─────────────────────────────────────────────────

        [Fact]
        public void Format_Decimal_Amount_Returns12Digits()
        {
            Assert.Equal("000001234567", IsoType.AMOUNT.Format(12345.67m, 0));
        }

        [Fact]
        public void Format_Decimal_Numeric_DelegatesToLong()
        {
            Assert.Equal("000123", IsoType.NUMERIC.Format(123m, 6));
        }

        [Fact]
        public void Format_Decimal_ALPHA_FormatsAsString()
        {
            Assert.Equal("1.5  ", IsoType.ALPHA.Format(1.5m, 5));
        }

        [Fact]
        public void Format_Decimal_LLVAR_ReturnsString()
        {
            var result = IsoType.LLVAR.Format(12.5m, 0);
            Assert.Equal("12.5", result);
        }

        [Fact]
        public void Format_Decimal_BINARY_ReturnsEmpty()
        {
            Assert.Equal("", IsoType.BINARY.Format(1m, 4));
            Assert.Equal("", IsoType.LLBIN.Format(1m, 0));
        }

        [Fact]
        public void Format_Decimal_UnsupportedType_Throws()
        {
            Assert.Throws<ArgumentException>(() => IsoType.DATE10.Format(123m, 10));
            Assert.Throws<ArgumentException>(() => IsoType.TIME.Format(123m, 6));
        }
    }
}
