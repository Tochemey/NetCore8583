using System;
using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Unit tests for <see cref="Date14ParseInfo"/>.
    ///
    /// ASCII format: yyyyMMddHHmmss (14 characters, 4-digit year)
    ///
    /// Binary format: 7 BCD bytes (yy yy MM dd HH mm ss)
    ///   Each byte = one two-digit BCD value.
    ///   Year = tens[0] * 100 + tens[1], e.g. {0x20,0x26,...} → 2026
    /// </summary>
    public class TestDate14ParseInfo
    {
        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

        // ═══════════════════════════════════════════════════════════════════════
        // Parse (ASCII)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Parse_ReturnsCorrectDateTime()
        {
            // "20260316143000" → 2026-03-16 14:30:00
            var fpi = new Date14ParseInfo();
            var val = fpi.Parse(1, Ascii("20260316143000"), 0, null);
            Assert.Equal(IsoType.DATE14, val.Type);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
            Assert.Equal(14, dt.Hour);
            Assert.Equal(30, dt.Minute);
            Assert.Equal(0, dt.Second);
        }

        [Fact]
        public void Parse_EndOfMillennium()
        {
            // "19991231235959" → 1999-12-31 23:59:59
            var fpi = new Date14ParseInfo();
            var val = fpi.Parse(1, Ascii("19991231235959"), 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(1999, dt.Year);
            Assert.Equal(12, dt.Month);
            Assert.Equal(31, dt.Day);
            Assert.Equal(23, dt.Hour);
            Assert.Equal(59, dt.Minute);
            Assert.Equal(59, dt.Second);
        }

        [Fact]
        public void Parse_StartOfYear()
        {
            // "20000101000000" → 2000-01-01 00:00:00
            var fpi = new Date14ParseInfo();
            var val = fpi.Parse(1, Ascii("20000101000000"), 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(2000, dt.Year);
            Assert.Equal(1, dt.Month);
            Assert.Equal(1, dt.Day);
            Assert.Equal(0, dt.Hour);
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new Date14ParseInfo();
            var buf = Ascii("XXXX20260316143000");
            var val = fpi.Parse(1, buf, 4, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
        }

        [Fact]
        public void Parse_FutureTolerance_RoundTrip()
        {
            var soon = DateTime.UtcNow.AddSeconds(30);
            var buf = IsoType.DATE14.Format(soon).GetSignedBytes();
            var val = new Date14ParseInfo().Parse(0, buf, 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(soon.Month, dt.Month);
            Assert.Equal(soon.Day, dt.Day);
            Assert.Equal(soon.Hour, dt.Hour);
            Assert.Equal(soon.Minute, dt.Minute);
            Assert.Equal(soon.Second, dt.Second);
        }

        [Fact]
        public void Parse_NegativePosition_Throws()
        {
            var fpi = new Date14ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("20260316143000"), -1, null));
        }

        [Fact]
        public void Parse_InsufficientData_Throws()
        {
            var fpi = new Date14ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("2026031614"), 0, null));
        }

        [Fact]
        public void Parse_InsufficientDataFromOffset_Throws()
        {
            var fpi = new Date14ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("20260316143000"), 2, null));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary
        // 7 BCD bytes: (yy)(yy)(MM)(dd)(HH)(mm)(ss)
        // year = tens[0]*100 + tens[1]
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_ReturnsCorrectDateTime()
        {
            // {0x20,0x26,0x03,0x16,0x14,0x30,0x00} → 2026-03-16 14:30:00
            var fpi = new Date14ParseInfo();
            var buf = new sbyte[] { 0x20, 0x26, 0x03, 0x16, 0x14, 0x30, 0x00 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.DATE14, val.Type);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
            Assert.Equal(14, dt.Hour);
            Assert.Equal(30, dt.Minute);
            Assert.Equal(0, dt.Second);
        }

        [Fact]
        public void ParseBinary_EndOfMillennium()
        {
            // {0x19,0x99,0x12,0x31,0x23,0x59,0x59} → 1999-12-31 23:59:59
            var fpi = new Date14ParseInfo();
            var buf = new sbyte[] { 0x19, unchecked((sbyte) 0x99), 0x12, 0x31, 0x23, 0x59, 0x59 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(1999, dt.Year);
            Assert.Equal(12, dt.Month);
            Assert.Equal(31, dt.Day);
            Assert.Equal(23, dt.Hour);
            Assert.Equal(59, dt.Minute);
            Assert.Equal(59, dt.Second);
        }

        [Fact]
        public void ParseBinary_StartOfMillennium()
        {
            // {0x20,0x00,0x01,0x01,0x00,0x00,0x00} → 2000-01-01 00:00:00
            var fpi = new Date14ParseInfo();
            var buf = new sbyte[] { 0x20, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(2000, dt.Year);
            Assert.Equal(1, dt.Month);
            Assert.Equal(1, dt.Day);
        }

        [Fact]
        public void ParseBinary_WithOffset()
        {
            var fpi = new Date14ParseInfo();
            var buf = new sbyte[] { 0x00, 0x00, 0x20, 0x26, 0x03, 0x16, 0x14, 0x30, 0x00 };
            var val = fpi.ParseBinary(1, buf, 2, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
        }

        [Fact]
        public void ParseBinary_NegativePosition_Throws()
        {
            var fpi = new Date14ParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[7], -1, null));
        }

        [Fact]
        public void ParseBinary_InsufficientData_Throws()
        {
            var fpi = new Date14ParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x20, 0x26, 0x03, 0x16 }, 0, null));
        }

        [Fact]
        public void ParseBinary_InsufficientDataFromOffset_Throws()
        {
            var fpi = new Date14ParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[7], 2, null));
        }

        [Fact]
        public void ParseBinary_RoundTrip_MatchesAsciiParse()
        {
            var soon = DateTime.UtcNow.AddSeconds(30);
            var asciiStr = IsoType.DATE14.Format(soon);
            var asciiVal = new Date14ParseInfo().Parse(0, asciiStr.GetSignedBytes(), 0, null);
            var asciiDt = (DateTime) asciiVal.Value;

            int y1 = asciiDt.Year / 100, y2 = asciiDt.Year % 100;
            var bcdBuf = new sbyte[]
            {
                (sbyte) (((y1 / 10) << 4) | (y1 % 10)),
                (sbyte) (((y2 / 10) << 4) | (y2 % 10)),
                (sbyte) (((asciiDt.Month / 10) << 4) | (asciiDt.Month % 10)),
                (sbyte) (((asciiDt.Day / 10) << 4) | (asciiDt.Day % 10)),
                (sbyte) (((asciiDt.Hour / 10) << 4) | (asciiDt.Hour % 10)),
                (sbyte) (((asciiDt.Minute / 10) << 4) | (asciiDt.Minute % 10)),
                (sbyte) (((asciiDt.Second / 10) << 4) | (asciiDt.Second % 10))
            };

            var binVal = new Date14ParseInfo().ParseBinary(0, bcdBuf, 0, null);
            var binDt = (DateTime) binVal.Value;

            Assert.Equal(asciiDt.Year, binDt.Year);
            Assert.Equal(asciiDt.Month, binDt.Month);
            Assert.Equal(asciiDt.Day, binDt.Day);
            Assert.Equal(asciiDt.Hour, binDt.Hour);
            Assert.Equal(asciiDt.Minute, binDt.Minute);
            Assert.Equal(asciiDt.Second, binDt.Second);
        }
    }
}
