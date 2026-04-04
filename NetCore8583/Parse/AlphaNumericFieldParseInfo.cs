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
    ///     This is the common abstract superclass to parse ALPHA and NUMERIC field types.
    /// </summary>
    public abstract class AlphaNumericFieldParseInfo : FieldParseInfo
    {
        /// <summary>Initializes parse info for a fixed-length alpha/numeric field.</summary>
        /// <param name="isoType">The ISO type (ALPHA or NUMERIC).</param>
        /// <param name="length">The fixed field length.</param>
        protected AlphaNumericFieldParseInfo(IsoType isoType,
            int length) : base(isoType,
            length)
        {
        }

        /// <inheritdoc />
        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid ALPHA/NUM field {field} position {pos}");
            if (pos + Length > buf.Length)
                throw new ParseException(
                    $"Insufficient data for {IsoType} field {field} of length {Length}, pos {pos}");
            try
            {
                var v = buf.ToString(pos,
                    Length,
                    Encoding);

                if (v.Length != Length)
                    v = buf.ToString(pos,
                        buf.Length - pos,
                        Encoding).Substring(0,
                        Length);

                if (custom == null)
                    return new IsoValue(IsoType,
                        v,
                        Length);

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