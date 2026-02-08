using System;
using NetCore8583.Extensions;

namespace NetCore8583.Codecs
{
    /// <summary>
    /// Custom field encoder/decoder for LLBIN/LLLBIN fields that contain long integers in BCD encoding.
    /// </summary>
    public class LongBcdCodec : ICustomBinaryField
    {
        /// <inheritdoc />
        public object DecodeField(string val) => long.Parse(val);

        /// <inheritdoc />
        public string EncodeField(object obj) => Convert.ToString(obj);

        /// <inheritdoc />
        public object DecodeBinaryField(sbyte[] bytes, int offset, int length) =>
            Bcd.DecodeToLong(bytes, offset, length * 2);

        /// <inheritdoc />
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