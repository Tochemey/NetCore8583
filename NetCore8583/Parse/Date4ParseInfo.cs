using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    /// <summary>
    ///     This class is used to parse fields of type DATE4.
    /// </summary>
    public class Date4ParseInfo : DateTimeParseInfo
    {
        public Date4ParseInfo() : base(IsoType.DATE4,
            4)
        {
        }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE4 field {field} position {pos}");
            if (pos + 4 > buf.Length) throw new ParseException($"Insufficient data for DATE4 field {field}, pos {pos}");

            int month, day, minute, seconds, milliseconds;

            //Set the month and the day in the date
            if (ForceStringDecoding)
            {
                var c = buf.ToString(pos,
                    2,
                    Encoding);
                month = Convert.ToInt32(c,
                    10);
                c = buf.ToString(pos + 2,
                    2,
                    Encoding);
                day = Convert.ToInt32(c,
                    10);
            }
            else
            {
                // month in .NET start at 1 since this a Java port
                month = (buf[pos] - 48) * 10 + buf[pos + 1] - 48;
                day = (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48;
            }

            var year = DateTime.Today.Year;
            var hour = minute = seconds = milliseconds = 0;

            var dt = new DateTime(year,
                month,
                day,
                hour,
                minute,
                seconds,
                milliseconds);

            if (TimeZoneInfo != null)
                dt = TimeZoneInfo.ConvertTime(dt,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                AdjustWithFutureTolerance(new DateTimeOffset(dt)).DateTime);
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            var tens = new int[2];
            var sbytes = buf;
            var start = 0;

            if (buf.Length - pos < 2)
                throw new ParseException($"Insufficient data to parse binary DATE4 field {field} pos {pos}");

            for (var i = pos; i < pos + tens.Length; i++)
                tens[start++] = ((sbytes[i] & 0xf0) >> 4) * 10 + (sbytes[i] & 0x0f);

            var calendar = new DateTime(DateTime.Now.Year,
                tens[0],
                tens[1],
                0,
                0,
                0).AddMilliseconds(0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                AdjustWithFutureTolerance(new DateTimeOffset(calendar)).DateTime);
        }
    }
}