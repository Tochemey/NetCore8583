using System;
using System.Text;

namespace NetCore8583.Util
{
    public static class ByteUtil
    {
        /// <summary>
        ///     Convert all unsigned bytes to signed bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static sbyte[] ToInt8(this byte[] bytes)
        {
            return Array.ConvertAll(bytes,
                b => unchecked((sbyte) b));
        }

        /// <summary>
        ///     Convert a signed bytes array to unsigned bytes array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] ToUint8(this sbyte[] bytes)
        {
            return Array.ConvertAll(bytes,
                a => (byte) a);
        }

        public static string ToString(this sbyte[] sbytes,
            int pos,
            int len,
            Encoding encoding)
        {
            unsafe
            {
                fixed (sbyte* pAsciiUpper = sbytes)
                {
                    var s = new string(pAsciiUpper,
                        pos,
                        len,
                        encoding);
                    return s;
                }
            }
        }

        public static string ToString(this sbyte[] sbytes,
            Encoding encoding)
        {
            unsafe
            {
                fixed (sbyte* pAsciiUpper = sbytes)
                {
                    var s = new string(pAsciiUpper,
                        0,
                        sbytes.Length,
                        encoding);
                    return s;
                }
            }
        }
    }
}