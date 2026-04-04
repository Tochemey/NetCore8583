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
    /// <summary>Parse info for DATE_EXP (yyMM, expiration date) fields.</summary>
    public class DateExpParseInfo : DateTimeParseInfo
    {
        /// <summary>Initializes parse info for DATE_EXP.</summary>
        public DateExpParseInfo() : base(IsoType.DATE_EXP,
            4)
        {
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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