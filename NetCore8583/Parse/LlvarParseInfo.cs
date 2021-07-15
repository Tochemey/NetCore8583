using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    public class LlvarParseInfo : FieldParseInfo
    {
        public LlvarParseInfo() : base(IsoType.LLVAR,
            0)
        {
        }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid LLVAR field {field} {pos}");
            if (pos + 2 > buf.Length) throw new ParseException($"Insufficient data for LLVAR header, pos {pos}");
            var len = DecodeLength(buf,
                pos,
                2);

            if (len < 0) throw new ParseException($"Invalid LLVAR length {len}, field {field} pos {pos}");

            if (len + pos + 2 > buf.Length)
                throw new ParseException($"Insufficient data for LLVAR field {field}, pos {pos}");

            string v;
            try
            {
                v = len == 0
                    ? string.Empty
                    : buf.ToString(pos + 2,
                        len,
                        Encoding);
            }
            catch (Exception)
            {
                throw new ParseException($"Insufficient data for LLVAR header, field {field} pos {pos}");
            }

            //This is new: if the String's length is different from the specified length in the
            //buffer, there are probably some extended characters. So we create a String from
            //the rest of the buffer, and then cut it to the specified length.
            if (v.Length != len)
                v = buf.ToString(pos + 2,
                    buf.Length - pos - 2,
                    Encoding).Substring(0,
                    len);

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

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            var sbytes = buf;

            if (pos < 0) throw new ParseException($"Invalid bin LLVAR field {field} pos {pos}");

            if (pos + 1 > buf.Length)
                throw new ParseException($"Insufficient data for bin LLVAR header, field {field} pos {pos}");

            var len = ((sbytes[pos] & 0xf0) >> 4) * 10 + (sbytes[pos] & 0x0f);

            if (len < 0) throw new ParseException($"Invalid bin LLVAR length {len}, field {field} pos {pos}");

            if (len + pos + 1 > buf.Length)
                throw new ParseException($"Insufficient data for bin LLVAR field {field}, pos {pos}");

            if (custom == null)
                return new IsoValue(IsoType,
                    buf.ToString(pos + 1,
                        len,
                        Encoding));

            var dec = custom.DecodeField(buf.ToString(pos + 1,
                len,
                Encoding));

            return dec == null
                ? new IsoValue(IsoType,
                    buf.ToString(pos + 1,
                        len,
                        Encoding))
                : new IsoValue(IsoType,
                    dec,
                    custom);
        }
    }
}