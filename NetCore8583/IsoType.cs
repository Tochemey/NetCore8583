using System;

namespace NetCore8583
{
    /// <summary>
    ///     IsoType
    /// </summary>
    public enum IsoType
    {
        /// <summary>
        ///     A fixed-length numeric value. It is zero-filled to the left.
        /// </summary>
        NUMERIC,

        /// <summary>
        ///     A fixed-length alphanumeric value. It is filled with spaces to the right.
        /// </summary>
        ALPHA,

        /// <summary>
        ///     A variable length alphanumeric value with a 2-digit header length.
        /// </summary>
        LLVAR,

        /// <summary>
        ///     A variable length alphanumeric value with a 3-digit header length.
        /// </summary>
        LLLVAR,

        /// <summary>
        ///     A date in format YYYYMMddHHmmss
        /// </summary>
        DATE14,

        /// <summary>
        ///     A date in format MMddHHmmss
        /// </summary>
        DATE10,

        /// <summary>
        ///     A date in format MMdd
        /// </summary>
        DATE4,

        /// <summary>
        ///     A date in format yyMM
        /// </summary>
        DATE_EXP,

        /// <summary>
        ///     Time of day in format HHmmss
        /// </summary>
        TIME,

        /// <summary>
        ///     An amount, expressed in cents with a fixed length of 12.
        /// </summary>
        AMOUNT,

        /// <summary>
        ///     Similar to ALPHA but holds byte arrays instead of strings.
        /// </summary>
        BINARY,

        /// <summary>
        ///     Similar to LLVAR but holds byte arrays instead of strings.
        /// </summary>
        LLBIN,

        /// <summary>
        ///     Similar to LLLVAR but holds byte arrays instead of strings.
        /// </summary>
        LLLBIN,

        /// <summary>
        ///     variable length with 4-digit header length.
        /// </summary>
        LLLLVAR,

        /// <summary>
        ///     variable length byte array with 4-digit header length.
        /// </summary>
        LLLLBIN,

        /// <summary>
        ///     Date in format yyMMddHHmmss.
        /// </summary>
        DATE12
    }

    /// <summary>
    ///     Helper class that helps check some properties on IsoType
    /// </summary>
    public static class IsoTypeHelper
    {
        /// <summary>
        ///     Checks whether an IsoType need length attribute
        /// </summary>
        /// <param name="isoType"></param>
        /// <returns></returns>
        public static bool NeedsLength(this IsoType isoType)
        {
            return isoType == IsoType.ALPHA || isoType == IsoType.NUMERIC || isoType == IsoType.BINARY;
        }

        /// <summary>
        ///     Gets the length of an IsoType
        /// </summary>
        /// <param name="isoType"></param>
        /// <returns></returns>
        public static int Length(this IsoType isoType)
        {
            switch (isoType)
            {
                case IsoType.ALPHA: return 0;
                case IsoType.AMOUNT: return 12;
                case IsoType.BINARY: return 0;
                case IsoType.DATE10: return 10;
                case IsoType.DATE12: return 12;
                case IsoType.DATE14: return 14;
                case IsoType.DATE4: return 4;
                case IsoType.DATE_EXP: return 4;
                case IsoType.LLBIN: return 0;
                case IsoType.NUMERIC: return 0;
                case IsoType.LLVAR: return 0;
                case IsoType.LLLVAR: return 0;
                case IsoType.TIME: return 6;
                case IsoType.LLLBIN: return 0;
                case IsoType.LLLLVAR: return 0;
                case IsoType.LLLLBIN: return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(isoType),
                        isoType,
                        null);
            }
        }

        /// <summary>
        ///     Format Date IsoType value
        /// </summary>
        /// <param name="isoType">date IsoType</param>
        /// <param name="dateTime">the IsoType value</param>
        /// <returns></returns>
        public static string Format(this IsoType isoType,
            DateTimeOffset dateTime)
        {
            switch (isoType)
            {
                case IsoType.DATE10: return dateTime.ToString("MMddHHmmss");
                case IsoType.DATE12: return dateTime.ToString("yyMMddHHmmss");
                case IsoType.DATE4: return dateTime.ToString("MMdd");
                case IsoType.DATE14: return dateTime.ToString("yyyyMMddHHmmss");
                case IsoType.DATE_EXP: return dateTime.ToString("yyMM");
                case IsoType.TIME: return dateTime.ToString("HHmmss");
                default: throw new ArgumentException("IsoType must be DATE10, DATE12, DATE4, DATE14, DATE_EXP or TIME");
            }
        }

        public static string Format(this IsoType isoType,
            string value,
            int length)
        {
            if (isoType == IsoType.ALPHA)
            {
                var c = new char[length];
                if (value == null) value = string.Empty;
                if (value.Length > length)
                    return value.Substring(0,
                        length);
                if (value.Length == length) return value;
                Array.Copy(value.ToCharArray(),
                    c,
                    value.Length);
                for (var i = value.Length; i < length; i++) c[i] = ' ';
                return new string(c);
            }

            if (isoType == IsoType.LLVAR || isoType == IsoType.LLLVAR || isoType == IsoType.LLLLVAR) return value;
            if (isoType == IsoType.NUMERIC)
            {
                var c = new char[length];
                var x = value.ToCharArray();
                if (x.Length > length)
                    throw new ArgumentException("Numeric value is larger than intended length: " + value + " LEN " +
                                                length);
                var lim = c.Length - x.Length;
                for (var i = 0; i < lim; i++) c[i] = '0';
                Array.Copy(x,
                    0,
                    c,
                    lim,
                    x.Length);
                return new string(c);
            }

            if (isoType == IsoType.AMOUNT)
            {
                var dec = decimal.Parse(value);
                var x = dec.ToString("0000000000.00").ToCharArray();
                var digits = new char[12];
                Array.Copy(x,
                    digits,
                    10);
                Array.Copy(x,
                    11,
                    digits,
                    10,
                    2);
                return new string(digits);
            }

            if (isoType == IsoType.BINARY)
            {
                if (value == null) value = "";
                if (value.Length > length)
                    return value.Substring(0,
                        length);
                var c = new char[length];
                var end = value.Length;
                if (value.Length % 2 == 1)
                {
                    c[0] = '0';
                    Array.Copy(value.ToCharArray(),
                        0,
                        c,
                        1,
                        value.Length);
                    end++;
                }
                else
                {
                    Array.Copy(value.ToCharArray(),
                        0,
                        c,
                        0,
                        value.Length);
                }

                for (var i = end; i < c.Length; i++) c[i] = '0';
                return new string(c);
            }

            if (isoType == IsoType.LLBIN || isoType == IsoType.LLLBIN || isoType == IsoType.LLLLBIN) return value;
            throw new ArgumentException("Cannot format String as " + isoType);
        }

        public static string Format(this IsoType isoType,
            long value,
            int length)
        {
            if (isoType == IsoType.NUMERIC)
            {
                var x = value.ToString().PadLeft(length,
                    '0');
                if (x.Length > length)
                    throw new ArgumentException("Numeric value is larger than intended length: " + value + " LEN " +
                                                length);
                return x;
            }

            if (isoType == IsoType.ALPHA || isoType == IsoType.LLVAR || isoType == IsoType.LLLVAR ||
                isoType == IsoType.LLLLVAR)
                return isoType.Format(Convert.ToString(value),
                    length);
            if (isoType == IsoType.AMOUNT) return value.ToString("0000000000") + "00";
            if (isoType == IsoType.BINARY || isoType == IsoType.LLBIN || isoType == IsoType.LLLBIN ||
                isoType == IsoType.LLLLBIN)
            {
                //TODO
            }

            throw new ArgumentException("Cannot format number as " + isoType);
        }

        public static string Format(this IsoType isoType,
            decimal value,
            int length)
        {
            if (isoType == IsoType.AMOUNT)
            {
                var x = value.ToString("0000000000.00").ToCharArray();
                var digits = new char[12];
                Array.Copy(x,
                    digits,
                    10);
                Array.Copy(x,
                    11,
                    digits,
                    10,
                    2);
                return new string(digits);
            }

            if (isoType == IsoType.NUMERIC)
                return isoType.Format(Convert.ToInt64(value),
                    length);
            if (isoType == IsoType.ALPHA || isoType == IsoType.LLVAR || isoType == IsoType.LLLVAR ||
                isoType == IsoType.LLLLVAR)
                return isoType.Format(Convert.ToString(value),
                    length);
            if (isoType == IsoType.BINARY || isoType == IsoType.LLBIN || isoType == IsoType.LLLBIN ||
                isoType == IsoType.LLLLBIN)
            {
                //TODO
            }

            throw new ArgumentException("Cannot format decimal as " + isoType);
        }
    }
}