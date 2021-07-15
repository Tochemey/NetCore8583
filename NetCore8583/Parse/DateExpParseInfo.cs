using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    public class DateExpParseInfo : DateTimeParseInfo
    {
        public DateExpParseInfo() : base(IsoType.DATE_EXP,
            4)
        {
        }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE_EXP field {field} position {pos}");
            if (pos + 4 > buf.Length)
                throw new ParseException($"Insufficient data for DATE_EXP field {field}, pos {pos}");

            int year, month;
            int minute, seconds;
            var hour = minute = seconds = 0;
            var day = 1;

            if (ForceStringDecoding)
            {
                year = DateTime.Today.Year - DateTime.Today.Year % 100 + Convert.ToInt32(buf.ToString(pos,
                    2,
                    Encoding), 10);

                month = Convert.ToInt32(buf.ToString(pos + 2,
                    2,
                    Encoding), 10);
            }
            else
            {
                year = DateTime.Today.Year - DateTime.Today.Year % 100 + (buf[pos] - 48) * 10 + buf[pos + 1] - 48;
                month = (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48;
            }

            var calendar = new DateTime(year,
                month,
                day,
                hour,
                minute,
                seconds);
            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                calendar);
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE_EXP field {field} position {pos}");
            if (pos + 2 > buf.Length)
                throw new ParseException($"Insufficient data for DATE_EXP field {field} pos {pos}");

            var tens = new int[2];
            var start = 0;

            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            var calendar = new DateTime(DateTime.Now.Year - DateTime.Now.Year % 100 + tens[0],
                tens[1],
                1,
                0,
                0,
                0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                calendar);
        }
    }
}