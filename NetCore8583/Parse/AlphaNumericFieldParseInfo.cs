using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    /// <summary>
    ///     This is the common abstract superclass to parse ALPHA and NUMERIC field types.
    /// </summary>
    public abstract class AlphaNumericFieldParseInfo : FieldParseInfo
    {
        protected AlphaNumericFieldParseInfo(IsoType isoType,
            int length) : base(isoType,
            length)
        {
        }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid ALPHA/NUM field {field} position {pos}");
            if (pos + Length > buf.Length)
                throw new ParseException(
                    $"Insufficient data for {IsoType} field {field} of length {Length}, pos {pos}");
            try
            {
                var v = buf.ToString(pos,
                    Length,
                    Encoding);

                if (v.Length != Length)
                    v = buf.ToString(pos,
                        buf.Length - pos,
                        Encoding).Substring(0,
                        Length);

                if (custom == null)
                    return new IsoValue(IsoType,
                        v,
                        Length);

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