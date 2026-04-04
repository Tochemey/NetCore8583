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
    /// <summary>
    ///     This class is used to parse fields of type DATE6.
    /// </summary>
    public class Date6ParseInfo : DateTimeParseInfo
    {
        /// <summary>Initializes parse info for DATE6 (yyMMdd).</summary>
        public Date6ParseInfo() : base(IsoType.DATE6, 6)
        {
        }

        /// <inheritdoc />
        public override IsoValue Parse(int field, sbyte[] buf, int pos, ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE6 field {field} position {pos}");
            if (pos + 6 > buf.Length) throw new ParseException($"Insufficient data for DATE6 field {field}, pos {pos}");

            int year, month, day, minute, seconds, milliseconds;

            //Set the month and the day in the date
            if (ForceStringDecoding)
            {
                year = Convert.ToInt32(buf.ToString(pos,
                    2,
                    Encoding), 10);
                month = Convert.ToInt32(buf.ToString(pos + 2,
                    2,
                    Encoding), 10) - 1;
                day = Convert.ToInt32(buf.ToString(pos + 4,
                    2,
                    Encoding), 10);
            }
            else
            {
                year = (buf[pos] - 48) * 10 + buf[pos + 1] - 48;
                month = (buf[pos + 2] - 48) * 10 + buf[pos + 3] - 48;
                day = (buf[pos + 4] - 48) * 10 + buf[pos + 5] - 48;
            }

            var hour = minute = seconds = milliseconds = 0;

            if (year > 50) year = 1900 + year;
            else year = 2000 + year;

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
                dt);
        }

        /// <inheritdoc />
        public override IsoValue ParseBinary(int field, sbyte[] buf, int pos, ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid DATE6 field {field} position {pos}");
            if (pos + 3 > buf.Length) throw new ParseException($"Insufficient data for DATE6 field {field}, pos {pos}");

            var tens = new int[3];
            var start = 0;

            for (var i = pos; i < pos + tens.Length; i++) tens[start++] = Bcd.ParseBcdLength(buf[i]);

            var year = tens[0] > 50 ? 1900 + tens[0] : 2000 + tens[0];
            var month = tens[1];
            var day = tens[2];
            var dt = new DateTime(year, month, day, 0, 0, 0, 0);

            if (TimeZoneInfo != null)
                dt = TimeZoneInfo.ConvertTime(dt,
                    TimeZoneInfo);

            return new IsoValue(IsoType,
                AdjustWithFutureTolerance(new DateTimeOffset(dt)).DateTime);
        }
    }
}