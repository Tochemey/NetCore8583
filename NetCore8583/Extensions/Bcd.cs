using System;
using System.Numerics;

namespace NetCore8583.Extensions
{
    /// <summary>Binary-coded decimal (BCD) encode/decode helpers for ISO 8583 numeric and amount fields.</summary>
    public static class Bcd
    {
        /// <summary>Decodes a BCD-encoded region of the buffer into a long. Supports up to 18 digits.</summary>
        /// <param name="buf">Buffer containing BCD data.</param>
        /// <param name="pos">Start position.</param>
        /// <param name="length">Number of BCD digits (not bytes).</param>
        /// <returns>The decoded long value.</returns>
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

        /// <summary>Encodes a string of decimal digits into BCD in the given buffer (two digits per byte, optional leading zero for odd length).</summary>
        /// <param name="value">String of digits (0-9).</param>
        /// <param name="buf">Output buffer; must be at least (value.Length + 1) / 2 bytes.</param>
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

        /// <summary>Decodes a BCD-encoded region into a <see cref="BigInteger"/> (for values exceeding long range).</summary>
        /// <param name="buf">Buffer containing BCD data.</param>
        /// <param name="pos">Start position.</param>
        /// <param name="length">Number of BCD digits (not bytes).</param>
        /// <returns>The decoded value.</returns>
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


        /// <summary>Converts a single BCD byte to a two-digit integer (e.g. 0x21 → 21).</summary>
        /// <param name="b">One BCD byte (high nibble = tens, low nibble = units).</param>
        /// <returns>Integer value 0–99.</returns>
        public static int ParseBcdLength(sbyte b)
        {
            return ((b & 0xf0) >> 4) * 10 + (b & 0xf);
        }
    }
}