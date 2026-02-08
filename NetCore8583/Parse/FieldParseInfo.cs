using System;
using System.Runtime.CompilerServices;
using System.Text;
using NetCore8583.Extensions;

namespace NetCore8583.Parse
{
    /// <summary>
    ///     This class stores the necessary information for parsing an ISO8583 field
    ///     inside a message.
    /// </summary>
    public abstract class FieldParseInfo
    {
        /// <summary>Initializes parse info for the given ISO type and fixed length (for fixed-length types).</summary>
        /// <param name="isoType">The ISO field type.</param>
        /// <param name="length">The fixed length for ALPHA/NUMERIC/BINARY; ignored for variable-length types.</param>
        protected FieldParseInfo(IsoType isoType,
            int length)
        {
            IsoType = isoType;
            Length = length;
        }

        /// <summary>The ISO type this parser handles.</summary>
        protected IsoType IsoType { get; }

        /// <summary>The fixed length for fixed-length types.</summary>
        protected int Length { get; }

        /// <summary>When true, variable-length headers are decoded as string digits using <see cref="Encoding"/> and <see cref="Radix"/> instead of ASCII digits.</summary>
        public bool ForceStringDecoding { get; set; }

        /// <summary>Radix used when decoding length headers (e.g. 10 for decimal, 16 for hex). Default is 10.</summary>
        public int Radix { get; set; } = 10;

        /// <summary>Optional custom decoder for this field.</summary>
        public ICustomField Decoder { get; set; }

        /// <summary>Character encoding for string decoding. Default is <see cref="Encoding.Default"/>.</summary>
        public Encoding Encoding { get; set; } = Encoding.Default;

        /// <summary>
        ///     Parses the character data from the buffer and returns the IsoValue with the correct data type in it.
        /// </summary>
        /// <param name="field">The field index, useful for error reporting.</param>
        /// <param name="buf">The full ISO message buffer.</param>
        /// <param name="pos">The starting position for the field data.</param>
        /// <param name="custom">An optional <see cref="ICustomField"/> to decode the field.</param>
        /// <returns>An <see cref="IsoValue"/> containing the parsed value.</returns>
        public abstract IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom);

        /// <summary>
        ///     Parses binary data from the buffer, creating and returning an IsoValue of the configured
        ///     type and length.
        /// </summary>
        /// <param name="field">The field index, useful for error reporting.</param>
        /// <param name="buf">The full ISO message buffer.</param>
        /// <param name="pos">The starting position for the field data.</param>
        /// <param name="custom">An optional <see cref="ICustomField"/> to decode the field.</param>
        /// <returns>An <see cref="IsoValue"/> containing the parsed value.</returns>
        public abstract IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom);

        /// <summary>
        /// Decodes a length header from the buffer at the given position.
        /// </summary>
        /// <param name="buf">The message buffer.</param>
        /// <param name="pos">Start position of the length digits.</param>
        /// <param name="digits">Number of length digits (2, 3, or 4).</param>
        /// <returns>The decoded length value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int DecodeLength(sbyte[] buf,
            int pos,
            int digits)
        {
            return DecodeLength(buf.AsSpan(), pos, digits);
        }

        /// <summary>
        ///     Decodes length header from a span.
        /// </summary>
        /// <param name="buf">Buffer span containing the length data.</param>
        /// <param name="pos">Starting position in the span.</param>
        /// <param name="digits">Number of digits in the length (2, 3, or 4).</param>
        /// <returns>Decoded length value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int DecodeLength(ReadOnlySpan<sbyte> buf,
            int pos,
            int digits)
        {
            if (ForceStringDecoding)
            {
                var string0 = buf.Slice(pos, digits).ToString(Encoding);
                return Convert.ToInt32(string0, Radix);
            }

            return digits switch
            {
                2 => (buf[pos] - 48) * 10 + (buf[pos + 1] - 48),
                3 => (buf[pos] - 48) * 100 + (buf[pos + 1] - 48) * 10 + (buf[pos + 2] - 48),
                4 => (buf[pos] - 48) * 1000 + (buf[pos + 1] - 48) * 100 + 
                     (buf[pos + 2] - 48) * 10 + (buf[pos + 3] - 48),
                _ => -1
            };
        }

        /// <summary>
        /// Creates a <see cref="FieldParseInfo"/> instance for the given ISO type and length.
        /// </summary>
        /// <param name="t">The ISO field type.</param>
        /// <param name="len">The fixed length for ALPHA, NUMERIC, or BINARY types; ignored for variable-length types.</param>
        /// <param name="encoding">The encoding to use for string decoding.</param>
        /// <returns>A parser instance for the specified type.</returns>
        /// <exception cref="ArgumentException">Thrown when the type is not supported.</exception>
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