using System;
using System.Numerics;

namespace NetCore8583.Util
{
    public static class Bcd
    {
        public static long DecodeToLong(sbyte[] buf,
            int pos,
            int length)
        {
            if (length > 18) throw new IndexOutOfRangeException("Buffer too big to decode as long");
            long l = 0;
            var power = 1L;
            for (var i = pos + length / 2 + length % 2 - 1; i >= pos; i--)
            {
                l += (buf[i] & 0x0f) * power;
                power *= 10L;
                l += ((buf[i] & 0xf0) >> 4) * power;
                power *= 10L;
            }

            return l;
        }

        public static void Encode(string value,
            sbyte[] buf)
        {
            var charpos = 0; //char where we start
            var bufpos = 0;
            if (value.Length % 2 == 1)
            {
                //for odd lengths we encode just the first digit in the first byte
                buf[0] = (sbyte) (value[0] - 48);
                charpos = 1;
                bufpos = 1;
            }

            //encode the rest of the string
            while (charpos < value.Length)
            {
                buf[bufpos] = (sbyte) (((value[charpos] - 48) << 4) | (value[charpos + 1] - 48));
                charpos += 2;
                bufpos++;
            }
        }

        public static BigInteger DecodeToBigInteger(sbyte[] buf,
            int pos,
            int length)
        {
            var digits = new char[length];
            var start = 0;
            var i = pos;
            if (length % 2 != 0)
            {
                digits[start++] = (char) ((buf[i] & 0x0f) + 48);
                i++;
            }

            for (; i < pos + length / 2 + length % 2; i++)
            {
                digits[start++] = (char) (((buf[i] & 0xf0) >> 4) + 48);
                digits[start++] = (char) ((buf[i] & 0x0f) + 48);
            }

            return BigInteger.Parse(new string(digits));
        }


        /// <summary>
        ///     Convert two bytes of BCD length to an int,
        ///     e.g. 0x4521 into 4521, starting at the specified offset.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int ParseBcdLength(sbyte b)
        {
            return ((b & 0xf0) >> 4) * 10 + (b & 0xf);
        }
    }
}