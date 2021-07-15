using System;
using System.Text;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    public class LlbinParseInfo : FieldParseInfo
    {
        public LlbinParseInfo() : base(IsoType.LLBIN,
            0)
        {
        }

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