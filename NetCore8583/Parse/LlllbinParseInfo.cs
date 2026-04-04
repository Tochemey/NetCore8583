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
    /// <summary>Parse info for LLLLBIN (variable-length binary with 4-digit length header) fields.</summary>
    public class LlllbinParseInfo : FieldParseInfo
    {
        /// <summary>Initializes parse info for LLLLBIN (4-digit length, then binary data).</summary>
        public LlllbinParseInfo() : base(IsoType.LLLLBIN,
            0)
        {
        }

        /// <inheritdoc />
        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid LLLLBIN field {field} pos {pos}");
            if (pos + 4 > buf.Length) throw new ParseException($"Insufficient LLLLBIN header field {field}");

            var l = DecodeLength(buf,
                pos,
                4);

            if (l < 0) throw new ParseException($"Invalid LLLLBIN length {l} field {field} pos {pos}");

            if (l + pos + 4 > buf.Length)
                throw new ParseException($"Insufficient data for LLLLBIN field {field}, pos {pos}");
            var binval = l == 0
                ? new sbyte[0]
                : HexCodec.HexDecode(buf.ToString(pos + 4,
                    l,
                    Encoding.Default));
            if (custom == null)
                return new IsoValue(IsoType,
                    binval,
                    binval.Length);

            if (custom is ICustomBinaryField customBinaryField)
                try
                {
                    var dec = customBinaryField.DecodeBinaryField(buf,
                        pos + 4,
                        l);
                    return dec == null
                        ? new IsoValue(IsoType,
                            binval,
                            binval.Length)
                        : new IsoValue(IsoType,
                            dec,
                            0,
                            custom);
                }
                catch (Exception)
                {
                    throw new ParseException($"Insufficient data for LLLLBIN field {field}, pos {pos}");
                }

            try
            {
                var dec = custom.DecodeField(l == 0
                    ? ""
                    : buf.ToString(pos + 4,
                        l,
                        Encoding.Default));

                return dec == null
                    ? new IsoValue(IsoType,
                        binval,
                        binval.Length)
                    : new IsoValue(IsoType,
                        dec,
                        l,
                        custom);
            }
            catch (Exception)
            {
                throw new ParseException($"Insufficient data for LLLLBIN field {field}, pos {pos}");
            }
        }

        /// <inheritdoc />
        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid bin LLLLBIN field {field} pos {pos}");
            if (pos + 2 > buf.Length) throw new ParseException($"Insufficient LLLLBIN header field {field}");

            var l = ((buf[pos] & 0xf0) >> 4) * 1000 + (buf[pos] & 0x0f) * 100
                                                    + ((buf[pos + 1] & 0xf0) >> 4) * 10 + (buf[pos + 1] & 0x0f);

            if (l < 0) throw new ParseException($"Invalid LLLLBIN length {l} field {field} pos {pos}");
            if (l + pos + 2 > buf.Length)
                throw new ParseException(
                    $"Insufficient data for bin LLLLBIN field {field}, pos {pos} requires {l}, only {buf.Length - pos + 1} available");

            var v = new sbyte[l];
            Array.Copy(buf,
                pos + 2,
                v,
                0,
                l);

            if (custom == null)
                return new IsoValue(IsoType,
                    v);

            if (custom is ICustomBinaryField customBinaryField)
                try
                {
                    var dec = customBinaryField.DecodeBinaryField(buf,
                        pos + 2,
                        l);
                    return dec == null
                        ? new IsoValue(IsoType,
                            v,
                            v.Length)
                        : new IsoValue(IsoType,
                            dec,
                            l,
                            custom);
                }
                catch (Exception)
                {
                    throw new ParseException($"Insufficient data for LLLLBIN field {field}, pos {pos}");
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