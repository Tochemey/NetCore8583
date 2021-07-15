using System;
using NetCore8583.Util;

namespace NetCore8583.Parse
{
    /// <summary>
    ///     This class is used to parse AMOUNT fields.
    /// </summary>
    public class AmountParseInfo : FieldParseInfo
    {
        public AmountParseInfo() : base(IsoType.AMOUNT,
            12)
        {
        }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid AMOUNT field {field} position {pos}");
            if (pos + 12 > buf.Length)
                throw new ParseException($"Insufficient data for AMOUNT field {field}, pos {pos}");

            var v = buf.ToString(pos,
                12,
                Encoding);

            try
            {
                var d = decimal.Parse(v) / 100;
                var dec = d;
                return new IsoValue(IsoType,
                    dec,
                    Length);
            }
            catch (FormatException)
            {
                throw new ParseException($"Cannot read amount '{v}' field {field} pos {pos}");
            }
            catch (Exception)
            {
                throw new ParseException($"Insufficient data for AMOUNT field {field}, pos {pos}");
            }
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            var digits = new char[13];
            digits[10] = '.';
            var start = 0;
            for (var i = pos; i < pos + 6; i++)
            {
                digits[start++] = (char) (((buf[i] & 0xf0) >> 4) + 48);
                digits[start++] = (char) ((buf[i] & 0x0f) + 48);
                if (start == 10) start++;
            }

            try
            {
                return new IsoValue(IsoType.AMOUNT,
                    decimal.Parse(new string(digits)));
            }
            catch (FormatException)
            {
                throw new ParseException($"Cannot read amount '{new string(digits)}' field {field} pos {pos}");
            }
            catch (Exception)
            {
                throw new ParseException($"Insufficient data for AMOUNT field {field}, pos {pos}");
            }
        }
    }
}