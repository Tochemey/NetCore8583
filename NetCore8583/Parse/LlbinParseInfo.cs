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
    /// <summary>Parse info for LLBIN (variable-length binary with 2-digit length header) fields.</summary>
    public class LlbinParseInfo : FieldParseInfo
    {
        /// <summary>Initializes parse info for LLBIN (2-digit length, then binary data).</summary>
        public LlbinParseInfo() : base(IsoType.LLBIN,
            0)
        {
        }

        /// <inheritdoc />
        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid LLBIN field {field} position {pos}");
            if (pos + 2 > buf.Length) throw new ParseException($"Invalid LLBIN field {field} position {pos}");
            var len = DecodeLength(buf,
                pos,
                2);
            if (len < 0) throw new ParseException($"Invalid LLBIN field {field} length {len} pos {pos}");

            if (len + pos + 2 > buf.Length)
                throw new ParseException(
                    $"Insufficient data for LLBIN field {field}, pos {pos} (LEN states '{buf.ToString(pos, 2, Encoding.Default)}')");

            var binval = len == 0
                ? new sbyte[0]
                : HexCodec.HexDecode(buf.ToString(pos + 2,
                    len,
                    Encoding.Default));

            if (custom == null)
                return new IsoValue(IsoType,
                    binval,
                    binval.Length);

            if (custom is ICustomBinaryField binaryField)
                try
                {
                    var dec = binaryField.DecodeBinaryField(buf,
                        pos + 2,
                        len);

                    if (dec == null)
                        return new IsoValue(IsoType,
                            binval,
                            binval.Length);

                    return new IsoValue(IsoType,
                        dec,
                        0,
                        custom);
                }
                catch (Exception)
                {
                    throw new ParseException(
                        $"Insufficient data for LLBIN field {field}, pos {pos} (LEN states '{buf.ToString(pos, 2, Encoding.Default)}')");
                }

            try
            {
                var dec = custom.DecodeField(buf.ToString(pos + 2,
                    len,
                    Encoding.Default));

                return dec == null
                    ? new IsoValue(IsoType,
                        binval,
                        binval.Length)
                    : new IsoValue(IsoType,
                        dec,
                        binval.Length,
                        custom);
            }
            catch (Exception)
            {
                throw new ParseException(
                    $"Insufficient data for LLBIN field {field}, pos {pos} (LEN states '{buf.ToString(pos, 2, Encoding.Default)}')");
            }
        }

        /// <inheritdoc />
        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid bin LLBIN field {field} position {pos}");
            if (pos + 1 > buf.Length) throw new ParseException($"Insufficient bin LLBIN header field {field}");

            var sbytes = buf;

            var l = ((sbytes[pos] & 0xf0) >> 4) * 10 + (sbytes[pos] & 0x0f);
            if (l < 0) throw new ParseException($"Invalid bin LLBIN length {l} pos {pos}");
            if (l + pos + 1 > buf.Length)
                throw new ParseException(
                    $"Insufficient data for bin LLBIN field {field}, pos {pos}: need {l}, only {buf.Length} available");

            var v = new sbyte[l];
            Array.Copy(sbytes,
                pos + 1,
                v,
                0,
                l);

            if (custom == null)
                return new IsoValue(IsoType,
                    v);


            if (custom is ICustomBinaryField binaryField)
                try
                {
                    var dec = binaryField.DecodeBinaryField(sbytes,
                        pos + 1,
                        l);
                    return dec == null
                        ? new IsoValue(IsoType,
                            v,
                            v.Length)
                        : new IsoValue(IsoType,
                            dec,
                            l,
                            binaryField);
                }
                catch (Exception)
                {
                    throw new ParseException($"Insufficient data for LLBIN field {field}, pos {pos} length {l}");
                }

            {
                var dec = custom.DecodeField(HexCodec.HexEncode(v,
                    0,
                    v.Length));
                return dec == null
                    ? new IsoValue(IsoType,
                        v)
                    : new IsoValue(IsoType,
                        dec,
                        custom);
            }
        }
    }
}