using System;
using System.Globalization;
using System.Numerics;
using NetCore8583.Extensions;

namespace NetCore8583.Codecs
{
    /// <summary>
    /// Custom field encoder/decoder for LLBIN/LLLBIN fields that contain <see cref="BigInteger"/> values in BCD encoding.
    /// </summary>
    public class BigIntBcdCodec : ICustomBinaryField
    {
        /// <inheritdoc />
        public object DecodeField(string val) => new BigInteger(Convert.ToInt32(val, 10));

        /// <inheritdoc />
        public string EncodeField(object obj) => obj.ToString();

        /// <inheritdoc />
        public object DecodeBinaryField(sbyte[] bytes, int offset, int length) =>
            Bcd.DecodeToBigInteger(bytes, offset, length * 2);

        /// <inheritdoc />
        public sbyte[] EncodeBinaryField(object val)
        {
            var value = (BigInteger) val;
            var s = value.ToString(NumberFormatInfo.InvariantInfo);
            var buf = new sbyte[s.Length / 2 + s.Length % 2];
            Bcd.Encode(
                s,
                buf);
            return buf;
        }
    }
}