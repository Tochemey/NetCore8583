using System;
using System.Text;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    /// <summary>
    ///     This class stores the necessary information for parsing an ISO8583 field
    ///     inside a message.
    /// </summary>
    public abstract class FieldParseInfo
    {
        protected FieldParseInfo(IsoType isoType,
            int length)
        {
            IsoType = isoType;
            Length = length;
        }

        protected IsoType IsoType { get; }
        protected int Length { get; }
        public bool ForceStringDecoding { get; set; }
        public int Radix { get; set; } = 10;
        public ICustomField Decoder { get; set; }
        public Encoding Encoding { get; set; } = Encoding.Default;

        /// <summary>
        ///     Parses the character data from the buffer and returns the IsoValue with the correct data type in it.
        /// </summary>
        /// <param name="field">he field index, useful for error reporting.</param>
        /// <param name="buf">The full ISO message buffer.</param>
        /// <param name="pos">The starting position for the field data.</param>
        /// <param name="custom">A CustomField to decode the field.</param>
        /// <returns></returns>
        public abstract IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom);

        /// <summary>
        ///     Parses binary data from the buffer, creating and returning an IsoValue of the configured
        ///     type and length.
        /// </summary>
        /// <param name="field">he field index, useful for error reporting.</param>
        /// <param name="buf">The full ISO message buffer.</param>
        /// <param name="pos">The starting position for the field data.</param>
        /// <param name="custom">A CustomField to decode the field.</param>
        /// <returns></returns>
        public abstract IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom);

        /// <summary>
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="pos"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        protected int DecodeLength(sbyte[] buf,
            int pos,
            int digits)
        {
            if (ForceStringDecoding)
            {
                var string0 = buf.ToString(pos,
                    digits,
                    Encoding);
                return Convert.ToInt32(string0, Radix);
            }

            switch (digits)
            {
                case 2:
                    return (buf[pos] - 48) * 10 + (buf[pos + 1] - 48);

                case 3:
                    return (buf[pos] - 48) * 100 + (buf[pos + 1] - 48) * 10
                                                 + (buf[pos + 2] - 48);

                case 4:
                    return (buf[pos] - 48) * 1000 + (buf[pos + 1] - 48) * 100
                                                  + (buf[pos + 2] - 48) * 10 + (buf[pos + 3] - 48);
            }

            return -1;
        }

        public static FieldParseInfo GetInstance(IsoType t,
            int len,
            Encoding encoding)
        {
            FieldParseInfo fpi = null;
            switch (t)
            {
                case IsoType.ALPHA:
                    fpi = new AlphaParseInfo(len);
                    break;

                case IsoType.AMOUNT:
                    fpi = new AmountParseInfo();
                    break;

                case IsoType.BINARY:
                    fpi = new BinaryParseInfo(len);
                    break;

                case IsoType.DATE10:
                    fpi = new Date10ParseInfo();
                    break;

                case IsoType.DATE12:
                    fpi = new Date12ParseInfo();
                    break;

                case IsoType.DATE14:
                    fpi = new Date14ParseInfo();
                    break;

                case IsoType.DATE4:
                    fpi = new Date4ParseInfo();
                    break;

                case IsoType.DATE_EXP:
                    fpi = new DateExpParseInfo();
                    break;

                case IsoType.DATE6:
                    fpi = new Date6ParseInfo();
                    break;

                case IsoType.LLBIN:
                    fpi = new LlbinParseInfo();
                    break;

                case IsoType.LLLBIN:
                    fpi = new LllbinParseInfo();
                    break;

                case IsoType.LLLVAR:
                    fpi = new LllvarParseInfo();
                    break;

                case IsoType.LLVAR:
                    fpi = new LlvarParseInfo();
                    break;

                case IsoType.NUMERIC:
                    fpi = new NumericParseInfo(len);
                    break;

                case IsoType.TIME:
                    fpi = new TimeParseInfo();
                    break;

                case IsoType.LLLLVAR:
                    fpi = new LlllvarParseInfo();
                    break;

                case IsoType.LLLLBIN:
                    fpi = new LlllbinParseInfo();
                    break;
            }

            if (fpi == null) throw new ArgumentException($"Cannot parse type {t}");
            fpi.Encoding = encoding;
            return fpi;
        }
    }
}