using System;
using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Unit tests for <see cref="Date6ParseInfo"/>.
    ///
    /// ASCII format: yyMMdd (6 characters)
    ///   Year pivot: yy &gt; 50 → 19yy, yy &lt;= 50 → 20yy
    ///
    /// Binary format: 3 BCD bytes (yy MM dd), each byte = two BCD digits.
    /// </summary>
    public class TestDate6ParseInfo
    {
        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

        // ═══════════════════════════════════════════════════════════════════════
        // Parse (ASCII)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Parse_ReturnsCorrectDate()
        {
            // "260316" → year=26 → 2026, month=03, day=16
            var fpi = new Date6ParseInfo();
            var val = fpi.Parse(1, Ascii("260316"), 0, null);
            Assert.Equal(IsoType.DATE6, val.Type);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
        }

        [Fact]
        public void Parse_YearAbove50_Is1900sEra()
        {
            // "991231" → year=99 → 1999, month=12, day=31
            var fpi = new Date6ParseInfo();
            var val = fpi.Parse(1, Ascii("991231"), 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(1999, dt.Year);
            Assert.Equal(12, dt.Month);
            Assert.Equal(31, dt.Day);
        }

        [Fact]
        public void Parse_YearExactly50_Is2050()
        {
            // "500101" → year=50 → 2050, month=01, day=01
            var fpi = new Date6ParseInfo();
            var val = fpi.Parse(1, Ascii("500101"), 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(2050, dt.Year);
        }

        [Fact]
        public void Parse_YearExactly51_Is1951()
        {
            // "510601" → year=51 → 1951
            var fpi = new Date6ParseInfo();
            var val = fpi.Parse(1, Ascii("510601"), 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(1951, dt.Year);
        }

        [Fact]
        public void Parse_RoundTrip()
        {
            // Verify that Parse produces the same string as formatting
            var fpi = new Date6ParseInfo();
            var val = fpi.Parse(1, Ascii("171231"), 0, null);
            Assert.Equal("171231", val.ToString());
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new Date6ParseInfo();
            var buf = Ascii("XXXXXX260316");
            var val = fpi.Parse(1, buf, 6, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
        }

        [Fact]
        public void Parse_TimeComponentsAreZero()
        {
            var fpi = new Date6ParseInfo();
            var val = fpi.Parse(1, Ascii("260316"), 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(0, dt.Hour);
            Assert.Equal(0, dt.Minute);
            Assert.Equal(0, dt.Second);
            Assert.Equal(0, dt.Millisecond);
        }

        [Fact]
        public void Parse_NegativePosition_Throws()
        {
            var fpi = new Date6ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("260316"), -1, null));
        }

        [Fact]
        public void Parse_InsufficientData_Throws()
        {
            var fpi = new Date6ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("2603"), 0, null));
        }

        [Fact]
        public void Parse_InsufficientDataFromOffset_Throws()
        {
            var fpi = new Date6ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("260316"), 1, null));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary
        // Each byte is one BCD-encoded two-digit value: 0x26 → 26, 0x03 → 3, 0x16 → 16
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_ReturnsCorrectDate()
        {
            // 0x26=year 26 → 2026, 0x03=month 3, 0x16=day 16
            var fpi = new Date6ParseInfo();
            var buf = new sbyte[] { 0x26, 0x03, 0x16 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.DATE6, val.Type);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
        }

        [Fact]
        public void ParseBinary_YearAbove50_Is1900sEra()
        {
            // 0x99=99 → 1999, 0x12=12, 0x31=31
            var fpi = new Date6ParseInfo();
            var buf = new sbyte[] { unchecked((sbyte) 0x99), 0x12, 0x31 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(1999, dt.Year);
            Assert.Equal(12, dt.Month);
            Assert.Equal(31, dt.Day);
        }

        [Fact]
        public void ParseBinary_RoundTrip()
        {
            var fpi = new Date6ParseInfo();
            var buf = new sbyte[] { 0x17, 0x12, 0x31 };
            var val = fpi.ParseBinary(2, buf, 0, null);
            Assert.Equal("171231", val.ToString());
        }

        [Fact]
        public void ParseBinary_WithOffset()
        {
            var fpi = new Date6ParseInfo();
            var buf = new sbyte[] { 0x00, 0x00, 0x26, 0x03, 0x16 };
            var val = fpi.ParseBinary(1, buf, 2, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
        }

        [Fact]
        public void ParseBinary_NegativePosition_Throws()
        {
            var fpi = new Date6ParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x26, 0x03, 0x16 }, -1, null));
        }

        [Fact]
        public void ParseBinary_InsufficientData_Throws()
        {
            var fpi = new Date6ParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x26, 0x03 }, 0, null));
        }
    }
}
