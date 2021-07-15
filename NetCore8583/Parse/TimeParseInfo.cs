using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    public class TimeParseInfo : DateTimeParseInfo
    {
        public TimeParseInfo() : base(IsoType.TIME,
            6)
        {
        }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid TIME field {field} pos {pos}");
            if (pos + 6 > buf.Length) throw new ParseException($"Insufficient data for TIME field {field}, pos {pos}");
            DateTime calendar;
            if (ForceStringDecoding)
            {
                var hour = Convert.ToInt32(buf.ToString(pos,
                        2,
                        Encoding),
                    10);
                var minute = Convert.ToInt32(buf.ToString(pos + 2,
                        2,
                        Encoding),
                    10);
                var seconds = Convert.ToInt32(buf.ToString(pos + 4,
                    2,
                    Encoding), 10);
                calendar = new DateTime(DateTime.Today.Year,
                    DateTime.Today.Month,
                    DateTime.Today.Day,
                    hour,
                    minute,
                    seconds);
            }
            else
            {
                var sbytes = buf;
                calendar = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                    (sbytes[pos] - 48) * 10 + sbytes[pos + 1] - 48,
                    (sbytes[pos + 2] - 48) * 10 + sbytes[pos + 3] - 48,
                    (sbytes[pos + 4] - 48) * 10 + sbytes[pos + 5] - 48);
            }

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
            if (pos < 0) throw new ParseException($"Invalid bin TIME field {field} pos {pos}");
            if (pos + 3 > buf.Length)
                throw new ParseException($"Insufficient data for bin TIME field {field}, pos {pos}");
            var sbytes = buf;
            var tens = new int[3];
            var start = 0;
            for (var i = pos; i < pos + 3; i++) tens[start++] = ((sbytes[i] & 0xf0) >> 4) * 10 + (sbytes[i] & 0x0f);

            var calendar = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                tens[0],
                tens[1],
                tens[2]);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                calendar);
        }
    }
}