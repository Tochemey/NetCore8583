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
        public static sbyte[] ToSignedBytes(this byte[] bytes)
        {
            return Array.ConvertAll(bytes,
                b => unchecked((sbyte)b));
        }

        /// <summary>
        ///     Convert a signed bytes array to unsigned bytes array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] ToUnsignedBytes(this sbyte[] bytes)
        {
            return Array.ConvertAll(bytes,
                a => (byte)a);
        }

        /// <summary>
        ///     Converts a byte array to string using the the underling signed byte array using the ANSI encoding
        /// </summary>
        /// <param name="bytes">the bytes array</param>
        /// <returns></returns>
        public static string BytesToString(this byte[] bytes)
        {
            var sbytes = Array.ConvertAll(bytes,
                b => unchecked((sbyte)b));
            unsafe
            {
                string s;
                fixed (sbyte* pAsciiUpper = sbytes)
                {
                    s = new string(pAsciiUpper,
                        0,
                        sbytes.Length);
                    return s;
                }
            }
        }

        /// <summary>
        ///     Converts a byte array to string using the the underling signed byte array using the ANSI encoding.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <param name="pos">the starting position in the byte array</param>
        /// <param name="len">the number of bytes to</param>
        /// <returns></returns>
        public static string BytesToString(this byte[] bytes,
            int pos,
            int len)
        {
            var sbytes = Array.ConvertAll(bytes,
                b => unchecked((sbyte)b));
            unsafe
            {
                string s;
                fixed (sbyte* pAsciiUpper = sbytes)
                {
                    s = new string(pAsciiUpper,
                        pos,
                        len);
                    return s;
                }
            }
        }

        public static string BytesToString(this byte[] bytes,
            int pos,
            int len,
            Encoding encoding)
        {
            var sbytes = Array.ConvertAll(bytes,
                b => unchecked((sbyte)b));
            unsafe
            {
                string s;
                fixed (sbyte* pAsciiUpper = sbytes)
                {
                    s = new string(pAsciiUpper,
                        pos,
                        len,
                        encoding);
                    return s;
                }
            }
        }

        public static string SignedBytesToString(this sbyte[] sbytes,
            int pos,
            int len,
            Encoding encoding)
        {
            unsafe
            {
                string s;
                fixed (sbyte* pAsciiUpper = sbytes)
                {
                    s = new string(pAsciiUpper,
                        pos,
                        len,
                        encoding);
                    return s;
                }
            }
        }

        public static string SignedBytesToString(this sbyte[] sbytes,
            Encoding encoding)
        {
            unsafe
            {
                string s;
                fixed (sbyte* pAsciiUpper = sbytes)
                {
                    s = new string(pAsciiUpper,
                        0,
                        sbytes.Length,
                        encoding);
                    return s;
                }
            }
        }
    }
}