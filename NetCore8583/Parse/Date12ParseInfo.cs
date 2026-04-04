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
using NetCore8583.Extensions;

namespace NetCore8583.Parse
{
    /// <summary>Parse info for DATE12 (yyMMddHHmmss) fields.</summary>
    public class Date12ParseInfo : DateTimeParseInfo
    {
        /// <summary>Initializes parse info for DATE12.</summary>
        public Date12ParseInfo() : base(IsoType.DATE12,
            12)
        {
        }

        /// <inheritdoc />
        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE12 field {field} position {pos}");

            if (pos + 12 > buf.Length)
                throw new ParseException($"Insufficient data for DATE12 field {field}, pos {pos}");

            DateTime calendar;
            int year;
            if (ForceStringDecoding)
            {
                year = Convert.ToInt32(buf.ToString(pos,
                        2,
                        Encoding),
                    10);

                if (year > 50) year = 1900 + year;
                else year = 2000 + year;
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

                calendar = new DateTime(year,
                    month,
                    day,
                    hour,
                    min,
                    sec);
            }
            else
            {
                year = (buf[pos] - 48) * 10 + buf[pos + 1] - 48;

                if (year > 50) year = 1900 + year;
                else year = 2000 + year;

                calendar = new DateTime(year,
                    (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48,
                    (buf[pos + 4] - 48) * 10 + buf[pos + 5] - 48,
                    (buf[pos + 6] - 48) * 10 + buf[pos + 7] - 48,
                    (buf[pos + 8] - 48) * 10 + buf[pos + 9] - 48,
                    (buf[pos + 10] - 48) * 10 + buf[pos + 11] - 48);
            }

            calendar = calendar.AddMilliseconds(0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                AdjustWithFutureTolerance(new DateTimeOffset(calendar)).DateTime);
        }

        /// <inheritdoc />
        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE12 field {field} position {pos}");
            if (pos + 6 > buf.Length)
                throw new ParseException($"Insufficient data for DATE12 field {field}, pos {pos}");

            var tens = new int[6];
            var start = 0;
            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = ((buf[i] & 0xf0) >> 4) * 10 + (buf[i] & 0x0f);

            int year;
            if (tens[0] > 50) year = 1900 + tens[0];
            else year = 2000 + tens[0];

            var calendar = new DateTime(year,
                tens[1],
                tens[2],
                tens[3],
                tens[4],
                tens[5]).AddMilliseconds(0);

            if (TimeZoneInfo != null)
                calendar = TimeZoneInfo.ConvertTime(calendar,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                AdjustWithFutureTolerance(new DateTimeOffset(calendar)).DateTime);
        }
    }
}