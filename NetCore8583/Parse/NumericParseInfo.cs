using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    public class NumericParseInfo : AlphaNumericFieldParseInfo
    {
        public NumericParseInfo(int len) : base(IsoType.NUMERIC, len)
        {
        }

        public override IsoValue ParseBinary(int field, sbyte[] buf, int pos, ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid bin NUMERIC field {field} pos {pos}");
            if (pos + Length / 2 > buf.Length)
                throw new ParseException(
                    $"Insufficient data for bin {IsoType} field {field} of length {Length}, pos {pos}");

            //A long covers up to 18 digits
            if (Length < 19)
                return new IsoValue(IsoType.NUMERIC,
                    Bcd.DecodeToLong(buf,
                        pos,
                        Length),
                    Length);
            try
            {
                return new IsoValue(IsoType.NUMERIC,
                    Bcd.DecodeToBigInteger(buf,
                        pos,
                        Length),
                    Length);
            }
            catch (Exception)
            {
                throw new ParseException(
                    $"Insufficient data for bin {IsoType} field {field} of length {Length}, pos {pos}");
            }
        }
    }
}