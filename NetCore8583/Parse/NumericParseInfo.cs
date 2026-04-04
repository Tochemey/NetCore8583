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
    /// <summary>Parse info for fixed-length NUMERIC (BCD or ASCII digits) fields.</summary>
    public class NumericParseInfo : AlphaNumericFieldParseInfo
    {
        /// <summary>Initializes parse info for a fixed-length NUMERIC field.</summary>
        /// <param name="len">The fixed length in digits.</param>
        public NumericParseInfo(int len) : base(IsoType.NUMERIC, len)
        {
        }

        /// <inheritdoc />
        public override IsoValue ParseBinary(int field, sbyte[] buf, int pos, ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid bin NUMERIC field {field} pos {pos}");
            if (pos + Length / 2 > buf.Length)
                throw new ParseException(
                    $"Insufficient data for bin {IsoType} field {field} of length {Length}, pos {pos}");

            //A long covers up to 18 digits
            if (Length < 19)
                return new IsoValue(IsoType.NUMERIC,
                    Bcd.DecodeToLong(buf,
                        pos,
                        Length),
                    Length);
            try
            {
                return new IsoValue(IsoType.NUMERIC,
                    Bcd.DecodeToBigInteger(buf,
                        pos,
                        Length),
                    Length);
            }
            catch (Exception)
            {
                throw new ParseException(
                    $"Insufficient data for bin {IsoType} field {field} of length {Length}, pos {pos}");
            }
        }
    }
}