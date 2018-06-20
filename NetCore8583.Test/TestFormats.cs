using System;
using TimeZoneConverter;
using Xunit;

namespace NetCore8583.Test
{
    public class TestFormats
    {
        private DateTimeOffset date = DateTimeOffset.FromUnixTimeMilliseconds(96867296000L);

        [Fact]
        public void TestDateFormats()
        {
            // UTC-06:00 in honor to Enrique Zamudio
            //var tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            var tz = TZConvert.GetTimeZoneInfo("Central Standard Time");
            date = TimeZoneInfo.ConvertTime(date,
                tz);

            Assert.Equal("0125213456", IsoType.DATE10.Format(date));
            Assert.Equal("0125", IsoType.DATE4.Format(date));
            Assert.Equal("7301", IsoType.DATE_EXP.Format(date));
            Assert.Equal("213456", IsoType.TIME.Format(date));
            Assert.Equal("730125213456", IsoType.DATE12.Format(date));
            Assert.Equal("19730125213456", IsoType.DATE14.Format(date));

            // Now UTC
            date = TimeZoneInfo.ConvertTime(date,
                TimeZoneInfo.Utc);
            Assert.Equal("0126033456", IsoType.DATE10.Format(date));
            Assert.Equal("0126", IsoType.DATE4.Format(date));
            Assert.Equal("7301", IsoType.DATE_EXP.Format(date));
            Assert.Equal("033456", IsoType.TIME.Format(date));
            Assert.Equal("730126033456", IsoType.DATE12.Format(date));
            Assert.Equal("19730126033456", IsoType.DATE14.Format(date));

            //Now with GMT+1
            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo("W. Europe Standard Time");
            date = TimeZoneInfo.ConvertTime(date,
                timeZoneInfo);

            Assert.Equal("0126043456", IsoType.DATE10.Format(date));
            Assert.Equal("0126", IsoType.DATE4.Format(date));
            Assert.Equal("7301", IsoType.DATE_EXP.Format(date));
            Assert.Equal("043456", IsoType.TIME.Format(date));
            Assert.Equal("730126043456", IsoType.DATE12.Format(date));
            Assert.Equal("19730126043456", IsoType.DATE14.Format(date));
        }

        [Fact]
        public void TestNumericFormats()
        {
            Assert.Equal("000123", IsoType.NUMERIC.Format(123, 6));
            Assert.Equal("00hola", IsoType.NUMERIC.Format("hola", 6));
            Assert.Equal("000001234500", IsoType.AMOUNT.Format(12345, 0));
            Assert.Equal("000001234567", IsoType.AMOUNT.Format(decimal.Parse("12345.67"), 0));
            Assert.Equal("000000123456", IsoType.AMOUNT.Format("1234.56", 0));
        }

        [Fact]
        public void TestStringFormats()
        {
            Assert.Equal("hol", IsoType.ALPHA.Format("hola", 3));
            Assert.Equal("hola", IsoType.ALPHA.Format("hola", 4));
            Assert.Equal("hola  ", IsoType.ALPHA.Format("hola", 6));
            Assert.Equal("hola", IsoType.LLVAR.Format("hola", 0));
            Assert.Equal("hola", IsoType.LLLVAR.Format("hola", 0));
            Assert.Equal("HOLA", IsoType.LLLLVAR.Format("HOLA", 0));
        }
    }
}