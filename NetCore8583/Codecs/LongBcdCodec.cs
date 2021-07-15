using System;
using NetCore8583.Util;

namespace NetCore8583.Codecs
{
    /// <summary>
    ///     A custom field encoder/decoder to be used with LLBIN/LLLBIN fields
    ///     that contain Longs in BCD encoding.
    /// </summary>
    public class LongBcdCodec : ICustomBinaryField
    {
        public object DecodeField(string val) => long.Parse(val);

        public string EncodeField(object obj) => Convert.ToString(obj);

        public object DecodeBinaryField(sbyte[] bytes, int offset, int length) =>
            Bcd.DecodeToLong(bytes, offset, length * 2);

        public sbyte[] EncodeBinaryField(object obj)
        {
            var s = Convert.ToString(obj);
            if (s == null) return null;
            var buf = new sbyte[s.Length / 2 + s.Length % 2];
            Bcd.Encode(s, buf);
            return buf;
        }
    }
}