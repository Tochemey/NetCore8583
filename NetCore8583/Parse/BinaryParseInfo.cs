using System;
using System.Text;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    /// <summary>
    ///     This class is used to parse fields of type BINARY.
    /// </summary>
    public class BinaryParseInfo : FieldParseInfo
    {
        public BinaryParseInfo(int len) : base(IsoType.BINARY,
            len)
        {
        }

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