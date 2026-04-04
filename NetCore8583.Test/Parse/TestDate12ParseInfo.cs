// MIT License
//
// Copyright (c) 2020 - 2026 Arsene Tochemey Gandote
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Unit tests for <see cref="Date12ParseInfo"/>.
    ///
    /// ASCII format: yyMMddHHmmss (12 characters)
    ///   Year pivot: yy &gt; 50 → 19yy, yy &lt;= 50 → 20yy
    ///
    /// Binary format: 6 BCD bytes (yy MM dd HH mm ss), each byte = one two-digit BCD value.
    ///   e.g. {0x26,0x03,0x16,0x14,0x30,0x00} → 2026-03-16 14:30:00
    /// </summary>
    public class TestDate12ParseInfo
    {
        private static sbyte[] Ascii(string s) => s.GetSignedBytes(Encoding.ASCII);

        // ═══════════════════════════════════════════════════════════════════════
        // Parse (ASCII)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void Parse_ReturnsCorrectDateTime()
        {
            // "260316143000" → 2026-03-16 14:30:00
            var fpi = new Date12ParseInfo();
            var val = fpi.Parse(1, Ascii("260316143000"), 0, null);
            Assert.Equal(IsoType.DATE12, val.Type);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
            Assert.Equal(14, dt.Hour);
            Assert.Equal(30, dt.Minute);
            Assert.Equal(0, dt.Second);
        }

        [Fact]
        public void Parse_YearAbove50_Is1900sEra()
        {
            // "991231235959" → 1999-12-31 23:59:59
            var fpi = new Date12ParseInfo();
            var val = fpi.Parse(1, Ascii("991231235959"), 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(1999, dt.Year);
            Assert.Equal(12, dt.Month);
            Assert.Equal(31, dt.Day);
            Assert.Equal(23, dt.Hour);
            Assert.Equal(59, dt.Minute);
            Assert.Equal(59, dt.Second);
        }

        [Fact]
        public void Parse_YearExactly50_AdjustedByFutureTolerance()
        {
            // "500101000000" → year=50 → 2000+50=2050; but 2050 is far in the future
            // so AdjustWithFutureTolerance subtracts 1 year → 2049
            var fpi = new Date12ParseInfo();
            var val = fpi.Parse(1, Ascii("500101000000"), 0, null);
            var dt = (DateTime) val.Value;
            // Actual year depends on how far 2050-01-01 is from now; it will be 2049.
            Assert.Equal(2049, dt.Year);
        }

        [Fact]
        public void Parse_YearExactly51_Is1951()
        {
            // "510601120000" → 1951-06-01 12:00:00
            var fpi = new Date12ParseInfo();
            var val = fpi.Parse(1, Ascii("510601120000"), 0, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(1951, dt.Year);
        }

        [Fact]
        public void Parse_WithOffset()
        {
            var fpi = new Date12ParseInfo();
            var buf = Ascii("XXXX260316143000");
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
            var buf = IsoType.DATE12.Format(soon).GetSignedBytes();
            var val = new Date12ParseInfo().Parse(0, buf, 0, null);
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
            var fpi = new Date12ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("260316143000"), -1, null));
        }

        [Fact]
        public void Parse_InsufficientData_Throws()
        {
            var fpi = new Date12ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("26031614"), 0, null));
        }

        [Fact]
        public void Parse_InsufficientDataFromOffset_Throws()
        {
            var fpi = new Date12ParseInfo();
            Assert.Throws<ParseException>(() => fpi.Parse(1, Ascii("260316143000"), 2, null));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ParseBinary
        // 6 bytes, each BCD-encoded: ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ParseBinary_ReturnsCorrectDateTime()
        {
            // {0x26,0x03,0x16,0x14,0x30,0x00} → 2026-03-16 14:30:00
            var fpi = new Date12ParseInfo();
            var buf = new sbyte[] { 0x26, 0x03, 0x16, 0x14, 0x30, 0x00 };
            var val = fpi.ParseBinary(1, buf, 0, null);
            Assert.Equal(IsoType.DATE12, val.Type);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
            Assert.Equal(14, dt.Hour);
            Assert.Equal(30, dt.Minute);
            Assert.Equal(0, dt.Second);
        }

        [Fact]
        public void ParseBinary_YearAbove50_Is1900sEra()
        {
            // {0x99,0x12,0x31,0x23,0x59,0x59} → 1999-12-31 23:59:59
            var fpi = new Date12ParseInfo();
            var buf = new sbyte[]
            {
                unchecked((sbyte) 0x99), 0x12, 0x31,
                0x23, 0x59, 0x59
            };
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
        public void ParseBinary_WithOffset()
        {
            var fpi = new Date12ParseInfo();
            var buf = new sbyte[] { 0x00, 0x00, 0x26, 0x03, 0x16, 0x14, 0x30, 0x00 };
            var val = fpi.ParseBinary(1, buf, 2, null);
            var dt = (DateTime) val.Value;
            Assert.Equal(2026, dt.Year);
            Assert.Equal(3, dt.Month);
            Assert.Equal(16, dt.Day);
        }

        [Fact]
        public void ParseBinary_RoundTrip_MatchesAsciiParse()
        {
            var soon = DateTime.UtcNow.AddSeconds(30);
            var asciiStr = IsoType.DATE12.Format(soon);
            var asciiVal = new Date12ParseInfo().Parse(0, asciiStr.GetSignedBytes(), 0, null);
            var asciiDt = (DateTime) asciiVal.Value;

            // Build BCD buffer matching that date
            var yy = asciiDt.Year % 100;
            var bcdBuf = new sbyte[]
            {
                (sbyte) (((yy / 10) << 4) | (yy % 10)),
                (sbyte) (((asciiDt.Month / 10) << 4) | (asciiDt.Month % 10)),
                (sbyte) (((asciiDt.Day / 10) << 4) | (asciiDt.Day % 10)),
                (sbyte) (((asciiDt.Hour / 10) << 4) | (asciiDt.Hour % 10)),
                (sbyte) (((asciiDt.Minute / 10) << 4) | (asciiDt.Minute % 10)),
                (sbyte) (((asciiDt.Second / 10) << 4) | (asciiDt.Second % 10))
            };

            var binVal = new Date12ParseInfo().ParseBinary(0, bcdBuf, 0, null);
            var binDt = (DateTime) binVal.Value;

            Assert.Equal(asciiDt.Year, binDt.Year);
            Assert.Equal(asciiDt.Month, binDt.Month);
            Assert.Equal(asciiDt.Day, binDt.Day);
            Assert.Equal(asciiDt.Hour, binDt.Hour);
            Assert.Equal(asciiDt.Minute, binDt.Minute);
            Assert.Equal(asciiDt.Second, binDt.Second);
        }

        [Fact]
        public void ParseBinary_NegativePosition_Throws()
        {
            var fpi = new Date12ParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[6], -1, null));
        }

        [Fact]
        public void ParseBinary_InsufficientData_Throws()
        {
            var fpi = new Date12ParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[] { 0x26, 0x03, 0x16 }, 0, null));
        }

        [Fact]
        public void ParseBinary_InsufficientDataFromOffset_Throws()
        {
            var fpi = new Date12ParseInfo();
            Assert.Throws<ParseException>(() => fpi.ParseBinary(1, new sbyte[6], 2, null));
        }
    }
}
