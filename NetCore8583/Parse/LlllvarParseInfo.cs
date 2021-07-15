using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    public class LlllvarParseInfo : FieldParseInfo
    {
        public LlllvarParseInfo() : base(IsoType.LLLLVAR,
            0)
        {
        }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid LLLLVAR field {field} {pos}");
            if (pos + 4 > buf.Length) throw new ParseException($"Insufficient data for LLLLVAR header, pos {pos}");
            var len = DecodeLength(buf,
                pos,
                4);
            if (len < 0) throw new ParseException($"Invalid LLLLVAR length {len}, field {field} pos {pos}");
            if (len + pos + 4 > buf.Length)
                throw new ParseException($"Insufficient data for LLLLVAR field {field}, pos {pos}");

            string v;
            try
            {
                v = len == 0
                    ? ""
                    : buf.ToString(pos + 4,
                        len,
                        Encoding);
            }
            catch (Exception)
            {
                throw new ParseException($"Insufficient data for LLLLVAR header, field {field} pos {pos}");
            }

            //This is new: if the String's length is different from the specified
            // length in the buffer, there are probably some extended characters.
            // So we create a String from the rest of the buffer, and then cut it to
            // the specified length.
            if (v.Length != len)
                v = buf.ToString(pos + 4,
                    buf.Length - pos - 4,
                    Encoding).Substring(0,
                    len);
            if (custom == null)
                return new IsoValue(IsoType,
                    v,
                    len);

            var dec = custom.DecodeField(v);

            return dec == null
                ? new IsoValue(IsoType,
                    v,
                    len)
                : new IsoValue(IsoType,
                    dec,
                    len,
                    custom);
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            var sbytes = buf;

            if (pos < 0) throw new ParseException($"Invalid bin LLLLVAR field {field} pos {pos}");
            if (pos + 2 > buf.Length)
                throw new ParseException($"Insufficient data for bin LLLLVAR header, field {field} pos {pos}");

            var len = ((sbytes[pos] & 0xf0) >> 4) * 1000 + (sbytes[pos] & 0x0f) * 100 +
                      ((sbytes[pos + 1] & 0xf0) >> 4) * 10 + (sbytes[pos + 1] & 0x0f);

            if (len < 0) throw new ParseException($"Invalid bin LLLLVAR length {len}, field {field} pos {pos}");

            if (len + pos + 2 > sbytes.Length)
                throw new ParseException($"Insufficient data for bin LLLLVAR field {field}, pos {pos}");

            if (custom == null)
                return new IsoValue(IsoType,
                    buf.ToString(pos + 2,
                        len,
                        Encoding));

            var dec = custom.DecodeField(buf.ToString(pos + 2,
                len,
                Encoding));

            return dec == null
                ? new IsoValue(IsoType,
                    buf.ToString(pos + 2,
                        len,
                        Encoding))
                : new IsoValue(IsoType,
                    dec,
                    custom);
        }
    }
}