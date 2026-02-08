using System;
using System.Runtime.CompilerServices;
using System.Text;
using NetCore8583.Extensions;

namespace NetCore8583.Parse
{
    /// <summary>Parse info for LLLVAR (variable-length string with 3-digit length header) fields.</summary>
    public class LllvarParseInfo : FieldParseInfo
    {
        /// <summary>Initializes parse info for LLLVAR (3-digit length, then string data).</summary>
        public LllvarParseInfo() : base(IsoType.LLLVAR,
            0)
        {
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid LLLVAR field {field} pos {pos}");
            if (pos + 3 > buf.Length)
                throw new ParseException($"Insufficient data for LLLVAR header field {field} pos {pos}");

            var len = DecodeLength(buf,
                pos,
                3);

            if (len < 0)
                throw new ParseException(
                    $"Invalid LLLVAR length {len}({buf.ToString(pos, 3, Encoding.Default)}) field {field} pos {pos}");

            if (len + pos + 3 > buf.Length)
                throw new ParseException($"Insufficient data for LLLVAR field {field}, pos {pos}");

            string v;
            try
            {
                v = len == 0
                    ? string.Empty
                    : buf.ToString(pos + 3,
                        len,
                        Encoding);
            }
            catch (Exception)
            {
                throw new ParseException($"Insufficient data for LLLVAR header, field {field} pos {pos}");
            }

            //This is new: if the String's length is different from the specified length in the
            //buffer, there are probably some extended characters. So we create a String from
            //the rest of the buffer, and then cut it to the specified length.
            if (v.Length != len)
            {
                var fullString = buf.ToString(pos + 3, buf.Length - pos - 3, Encoding);
                v = fullString.AsSpan(0, Math.Min(len, fullString.Length)).ToString();
            }

            if (custom == null)
                return new IsoValue(IsoType,
                    v,
                    len);

            var decoded = custom.DecodeField(v);
            //If decode fails, return string; otherwise use the decoded object and its codec
            return decoded == null
                ? new IsoValue(IsoType,
                    v,
                    len)
                : new IsoValue(IsoType,
                    decoded,
                    len,
                    custom);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        /// <inheritdoc />
        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            var sbytes = buf;

            if (pos < 0) throw new ParseException($"Invalid bin LLLVAR field {field} pos {pos}");

            if (pos + 2 > buf.Length)
                throw new ParseException($"Insufficient data for bin LLLVAR header, field {field} pos {pos}");

            var len = (sbytes[pos] & 0x0f) * 100 + ((sbytes[pos + 1] & 0xf0) >> 4) * 10 + (sbytes[pos + 1] & 0x0f);
            if (len < 0) throw new ParseException($"Invalid bin LLLVAR length {len}, field {field} pos {pos}");

            if (len + pos + 2 > buf.Length)
                throw new ParseException($"Insufficient data for bin LLLVAR field {field}, pos {pos}");

            if (custom == null)
                return new IsoValue(IsoType,
                    buf.ToString(pos + 2,
                        len,
                        Encoding));

            var v = new IsoValue(IsoType,
                custom.DecodeField(buf.ToString(pos + 2,
                    len,
                    Encoding)),
                custom);

            if (v.Value == null)
                return new IsoValue(IsoType,
                    buf.ToString(pos + 2,
                        len,
                        Encoding.Default));
            return v;
        }
    }
}