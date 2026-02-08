using System;
using NetCore8583.Extensions;

namespace NetCore8583.Parse
{
    /// <summary>
    ///     This is the class used to parse ALPHA fields.
    /// </summary>
    public class AlphaParseInfo : AlphaNumericFieldParseInfo
    {
        /// <summary>Initializes parse info for a fixed-length ALPHA field.</summary>
        /// <param name="length">The fixed length in characters.</param>
        public AlphaParseInfo(int length) : base(IsoType.ALPHA,
            length)
        {
        }

        /// <inheritdoc />
        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid bin ALPHA field {field} position {pos}");
            if (pos + Length > buf.Length)
                throw new ParseException(
                    $"Insufficient data for bin {IsoType} field {field} of length {Length}, pos {pos}");
            try
            {
                string v;
                if (custom == null)
                {
                    v = buf.ToString(pos,
                        Length,
                        Encoding);
                    return new IsoValue(IsoType,
                        v,
                        Length);
                }

                v = buf.ToString(pos,
                    Length,
                    Encoding);

                var decoded = custom.DecodeField(v);
                return decoded == null
                    ? new IsoValue(IsoType,
                        v,
                        Length)
                    : new IsoValue(IsoType,
                        decoded,
                        Length,
                        custom);
            }
            catch (Exception)
            {
                throw new ParseException(
                    $"Insufficient data for {IsoType} field {field} of length {Length}, pos {pos}");
            }
        }
    }
}