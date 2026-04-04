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
using System.Text;
using NetCore8583.Extensions;

namespace NetCore8583.Parse
{
    /// <summary>
    ///     This class is used to parse fields of type BINARY.
    /// </summary>
    public class BinaryParseInfo : FieldParseInfo
    {
        /// <summary>Initializes parse info for a fixed-length BINARY field (hex in ASCII, raw bytes in binary).</summary>
        /// <param name="len">The fixed length in bytes.</param>
        public BinaryParseInfo(int len) : base(IsoType.BINARY,
            len)
        {
        }

        /// <inheritdoc />
        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid BINARY field {field} position {pos}");

            if (pos + Length * 2 > buf.Length)
                throw new ParseException($"Insufficient data for BINARY field {field} of length {Length}, pos {pos}");

            var s = buf.ToString(pos,
                Length * 2,
                Encoding.Default);
            //var binval = HexCodec.HexDecode(Encoding.ASCII.GetString(buf,
            //    pos,
            //    Length * 2));

            var binval = HexCodec.HexDecode(s);

            if (custom == null)
                return new IsoValue(IsoType,
                    binval,
                    binval.Length);

            s = buf.ToString(pos,
                Length * 2,
                Encoding);
            //var dec = custom.DecodeField(Encoding.GetString(buf,
            //    pos,
            //    Length * 2));
            var dec = custom.DecodeField(s);
            return dec == null
                ? new IsoValue(IsoType,
                    binval,
                    binval.Length)
                : new IsoValue(IsoType,
                    dec,
                    Length,
                    custom);
        }

        /// <inheritdoc />
        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid BINARY field {field} position {pos}");
            if (pos + Length > buf.Length)
                throw new ParseException($"Insufficient data for BINARY field {field} of length {Length}, pos {pos}");
            var v = new sbyte[Length];
            var sbytes = buf;
            Array.Copy(sbytes,
                pos,
                v,
                0,
                Length);
            if (custom == null)
                return new IsoValue(IsoType,
                    v,
                    Length);
            var dec = custom.DecodeField(HexCodec.HexEncode(v,
                0,
                v.Length));
            return dec == null
                ? new IsoValue(IsoType,
                    v,
                    Length)
                : new IsoValue(IsoType,
                    dec,
                    Length,
                    custom);
        }
    }
}