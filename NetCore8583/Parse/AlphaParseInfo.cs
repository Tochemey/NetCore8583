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
    ///     This is the class used to parse ALPHA fields.
    /// </summary>
    public class AlphaParseInfo : AlphaNumericFieldParseInfo
    {
        /// <summary>Initializes parse info for a fixed-length ALPHA field.</summary>
        /// <param name="length">The fixed length in characters.</param>
        public AlphaParseInfo(int length) : base(IsoType.ALPHA,
            length)
        {
        }

        /// <inheritdoc />
        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid bin ALPHA field {field} position {pos}");
            if (pos + Length > buf.Length)
                throw new ParseException(
                    $"Insufficient data for bin {IsoType} field {field} of length {Length}, pos {pos}");
            try
            {
                string v;
                if (custom == null)
                {
                    v = buf.ToString(pos,
                        Length,
                        Encoding);
                    return new IsoValue(IsoType,
                        v,
                        Length);
                }

                v = buf.ToString(pos,
                    Length,
                    Encoding);

                var decoded = custom.DecodeField(v);
                return decoded == null
                    ? new IsoValue(IsoType,
                        v,
                        Length)
                    : new IsoValue(IsoType,
                        decoded,
                        Length,
                        custom);
            }
            catch (Exception)
            {
                throw new ParseException(
                    $"Insufficient data for {IsoType} field {field} of length {Length}, pos {pos}");
            }
        }
    }
}