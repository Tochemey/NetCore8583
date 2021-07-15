using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    public class Date10ParseInfo : DateTimeParseInfo
    {
        public Date10ParseInfo() : base(IsoType.DATE10,
            10)
        {
        }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE10 field {field} position {pos}");
            if (pos + 10 > buf.Length)
                throw new ParseException($"Insufficient data for DATE10 field {field}, pos {pos}");

            DateTime calendar;
            if (ForceStringDecoding)
            {
                var month = Convert.ToInt32(buf.ToString(pos,
                        2,
                        Encoding),
                    10);
                var day = Convert.ToInt32(buf.ToString(pos + 2,
                        2,
                        Encoding),
                    10);
                var hour = Convert.ToInt32(buf.ToString(pos + 4,
                        2,
                        Encoding),
                    10);
                var min = Convert.ToInt32(buf.ToString(pos + 6,
                        2,
                        Encoding),
                    10);
                var sec = Convert.ToInt32(buf.ToString(pos + 8,
                        2,
                        Encoding),
                    10);
                calendar = new DateTime(DateTime.Today.Year,
                    month,
                    day,
                    hour,
                    min,
                    sec);
            }
            else
            {
                calendar = new DateTime(DateTime.Now.Year,
                    (buf[pos] - 48) * 10 + buf[pos + 1] - 48,
                    (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48,
                    (buf[pos + 4] - 48) * 10 + buf[pos + 5] - 48,
                    (buf[pos + 6] - 48) * 10 + buf[pos + 7] - 48,
                    (buf[pos + 8] - 48) * 10 + buf[pos + 9] - 48);
            }

            calendar = calendar.AddMilliseconds(0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                AdjustWithFutureTolerance(new DateTimeOffset(calendar)).DateTime);
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE10 field {field} position {pos}");
            if (pos + 5 > buf.Length)
                throw new ParseException($"Insufficient data for DATE10 field {field}, pos {pos}");
            var tens = new int[5];
            var start = 0;
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            var calendar = new DateTime(DateTime.Now.Year,
                tens[0],
                tens[1],
                tens[2],
                tens[3],
                tens[4]).AddMilliseconds(0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                AdjustWithFutureTolerance(new DateTimeOffset(calendar)).DateTime);
        }
    }
}