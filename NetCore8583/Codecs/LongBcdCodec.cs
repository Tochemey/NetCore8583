using NetCore8583.Util;
using System;

namespace NetCore8583.Codecs
{
    /// <summary>
    ///     A custom field encoder/decoder to be used with LLBIN/LLLBIN fields
    ///     that contain Longs in BCD encoding.
    /// </summary>
    public class LongBcdCodec : ICustomBinaryField
    {
        public object DecodeField(string val)
        {
            return long.Parse(val);
        }

        public string EncodeField(object obj)
        {
            return Convert.ToString(obj);
        }

        public object DecodeBinaryField(sbyte[] bytes,
            int offset,
            int length)
        {
            return Bcd.DecodeToLong(
                bytes,
                offset,
                length * 2);
        }

        public sbyte[] EncodeBinaryField(object obj)
        {
            var s = Convert.ToString(obj);
            var buf = new sbyte[s.Length / 2 + s.Length % 2];
            Bcd.Encode(
                s,
                buf);
            return buf;
        }
    }
}