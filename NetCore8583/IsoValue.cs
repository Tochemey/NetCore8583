using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using NetCore8583.Util;

namespace NetCore8583
{
    public class IsoValue : ICloneable
    {
        public IsoValue(IsoType t,
            object value,
            ICustomField custom = null)
        {
            if (t.NeedsLength())
                throw new ArgumentException("Fixed-value types must use constructor that specifies length");

            Encoder = custom;
            Type = t;
            Value = value;

            switch (Type)
            {
                case IsoType.LLVAR:
                case IsoType.LLLVAR:
                case IsoType.LLLLVAR:
                    if (Encoder == null)
                    {
                        Length = value.ToString().Length;
                    }
                    else
                    {
                        var enc = Encoder.EncodeField(value) ?? (value?.ToString() ?? string.Empty);
                        Length = enc.Length;
                    }

                    if (t == IsoType.LLVAR && Length > 99)
                        throw new ArgumentException("LLVAR can only hold values up to 99 chars");
                    if (t == IsoType.LLLVAR && Length > 999)
                        throw new ArgumentException("LLLVAR can only hold values up to 999 chars");
                    if (t == IsoType.LLLLVAR && Length > 9999)
                        throw new ArgumentException("LLLLVAR can only hold values up to 9999 chars");
                    break;
                case IsoType.LLBIN:
                case IsoType.LLLBIN:
                case IsoType.LLLLBIN:
                    if (Encoder == null)
                    {
                        if (value.GetType() == typeof(sbyte[]))
                        {
                            var obj = value;
                            Length = ((sbyte[]) obj).Length;
                        }
                        else
                        {
                            Length = value.ToString().Length / 2 + value.ToString().Length % 2;
                        }
                    }
                    else if (Encoder is ICustomBinaryField)
                    {
                        Length = ((ICustomBinaryField) Encoder).EncodeBinaryField(value).Length;
                    }
                    else
                    {
                        var enc = Encoder.EncodeField(value) ?? (value?.ToString() ?? string.Empty);
                        Length = enc.Length;
                    }

                    if (t == IsoType.LLBIN && Length > 99)
                        throw new ArgumentException("LLBIN can only hold values up to 99 chars");
                    if (t == IsoType.LLLBIN && Length > 999)
                        throw new ArgumentException("LLLBIN can only hold values up to 999 chars");
                    if (t == IsoType.LLLLBIN && Length > 9999)
                        throw new ArgumentException("LLLLBIN can only hold values up to 9999 chars");
                    break;
                default:
                    Length = Type.Length();
                    break;
            }
        }


        public IsoValue(IsoType t,
            object val,
            int len,
            ICustomField custom = null)
        {
            Type = t;
            Value = val;
            Length = len;
            Encoder = custom;
            if (Length == 0 && t.NeedsLength())
                throw new ArgumentException($"Length must be greater than zero for type {t} (value '{val}')");
            switch (t)
            {
                case IsoType.LLVAR:
                case IsoType.LLLVAR:
                case IsoType.LLLLVAR:
                    if (len == 0) Length = Encoder?.EncodeField(val).Length ?? val.ToString().Length;
                    if (t == IsoType.LLVAR && Length > 99)
                        throw new ArgumentException("LLVAR can only hold values up to 99 chars");
                    if (t == IsoType.LLLVAR && Length > 999)
                        throw new ArgumentException("LLLVAR can only hold values up to 999 chars");
                    if (t == IsoType.LLLLVAR && Length > 9999)
                        throw new ArgumentException("LLLLVAR can only hold values up to 9999 chars");
                    break;
                case IsoType.LLBIN:
                case IsoType.LLLBIN:
                case IsoType.LLLLBIN:
                    if (len == 0)
                    {
                        if (Encoder == null)
                        {
                            var obj = val;
                            Length = ((byte[]) obj).Length;
                        }
                        else if (Encoder is ICustomBinaryField)
                        {
                            var customBinaryField = (ICustomBinaryField) custom;
                            if (customBinaryField != null) Length = customBinaryField.EncodeBinaryField(Value).Length;
                        }
                        else
                        {
                            Length = Encoder.EncodeField(Value).Length;
                        }

                        Length = Encoder?.EncodeField(Value).Length ?? ((sbyte[]) val).Length;
                    }

                    if (t == IsoType.LLBIN && Length > 99)
                        throw new ArgumentException("LLBIN can only hold values up to 99 chars");
                    if (t == IsoType.LLLBIN && Length > 999)
                        throw new ArgumentException("LLLBIN can only hold values up to 999 chars");
                    if (t == IsoType.LLLLBIN && Length > 9999)
                        throw new ArgumentException("LLLLBIN can only hold values up to 9999 chars");
                    break;
            }
        }

        public ICustomField Encoder { get; }
        public IsoType Type { get; }
        public int Length { get; }
        public Encoding Encoding { get; set; } = Encoding.Default;
        public object Value { get; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            if (Value == null) return "ISOValue<null>";
            switch (Type)
            {
                case IsoType.NUMERIC:
                case IsoType.AMOUNT:
                    if (Type == IsoType.AMOUNT)
                        if (Value is decimal value)
                            return Type.Format(value,
                                12);
                        else
                            return Type.Format(Convert.ToDecimal(Value),
                                12);
                    else if (Value is BigInteger)
                        return Type.Format(Encoder == null ? Value.ToString() : Encoder.EncodeField(Value),
                            Length);
                    else if (Value is long l)
                        return Type.Format(l,
                            Length);
                    else
                        return Type.Format(Encoder == null ? Value.ToString() : Encoder.EncodeField(Value),
                            Length);
                case IsoType.ALPHA:
                    return Type.Format(Encoder == null ? Value.ToString() : Encoder.EncodeField(Value),
                        Length);
                case IsoType.LLVAR:
                case IsoType.LLLVAR:
                case IsoType.LLLLVAR: return Encoder == null ? Value.ToString() : Encoder.EncodeField(Value);
            }

            if (Value is DateTime dateTime) return Type.Format(dateTime);

            switch (Type)
            {
                case IsoType.BINARY:
                    if (Value is sbyte[] v1)
                    {
                        return Type.Format(Encoder == null
                                ? HexCodec.HexEncode(v1,
                                    0,
                                    v1.Length)
                                : Encoder.EncodeField(v1),
                            Length * 2);
                    }
                    else
                    {
                        return Type.Format(Encoder == null ? Value.ToString() : Encoder.EncodeField(Value),
                            Length * 2);
                    }
                case IsoType.LLBIN:
                case IsoType.LLLBIN:
                case IsoType.LLLLBIN:
                    if (Value is sbyte[] v)
                    {
                        return Encoder == null
                            ? HexCodec.HexEncode(v,
                                0,
                                v.Length)
                            : Encoder.EncodeField(v);
                    }
                    else
                    {
                        var s = Encoder == null ? Value.ToString() : Encoder.EncodeField(Value);
                        return s.Length % 2 == 1 ? $"0{s}" : s;
                    }
            }

            return Encoder == null ? Value.ToString() : Encoder.EncodeField(Value);
        }

        protected void WriteLengthHeader(int l,
            Stream outs,
            IsoType type,
            bool binary,
            bool forceStringEncoding)
        {
            var sbytes = new List<sbyte>();
            int digits;
            switch (type)
            {
                case IsoType.LLLLBIN:
                case IsoType.LLLLVAR:
                    digits = 4;
                    break;
                case IsoType.LLLBIN:
                case IsoType.LLLVAR:
                    digits = 3;
                    break;
                default:
                    digits = 2;
                    break;
            }

            if (binary)
            {
                switch (digits)
                {
                    case 4:
                        sbytes.Add((sbyte) (((l % 10000 / 1000) << 4) | (l % 1000 / 100)));
                        break;
                    case 3:
                        sbytes.Add((sbyte) (l / 100)); //00 to 09 automatically in BCD
                        break;
                }

                //BCD encode the rest of the length
                sbytes.Add((sbyte) (((l % 100 / 10) << 4) | (l % 10)));
            }
            else if (forceStringEncoding)
            {
                var lhead = Convert.ToString(l);
                var ldiff = digits - lhead.Length;
                switch (ldiff)
                {
                    case 1:
                        lhead = '0' + lhead;
                        break;
                    case 2:
                        lhead = "00" + lhead;
                        break;
                    case 3:
                        lhead = "000" + lhead;
                        break;
                }

                var bytes = lhead.GetSignedBytes(Encoding);
                foreach (var @sbyte in bytes) sbytes.Add(@sbyte);
            }
            else
            {
                //write the length in ASCII
                switch (digits)
                {
                    case 4:
                        sbytes.Add((sbyte) (l / 1000 + 48));
                        sbytes.Add((sbyte) (l % 1000 / 100 + 48));
                        break;
                    case 3:
                        sbytes.Add((sbyte) (l / 100 + 48));
                        break;
                }

                if (l >= 10) sbytes.Add((sbyte) (l % 100 / 10 + 48));
                else sbytes.Add(48);
                sbytes.Add((sbyte) (l % 10 + 48));
            }

            outs.Write(sbytes.ToArray().ToUnsignedBytes(),
                0,
                sbytes.Count);
        }

        public void Write(Stream outs,
            bool binary,
            bool forceStringEncoding)
        {
            switch (Type)
            {
                case IsoType.LLLVAR:
                case IsoType.LLVAR:
                case IsoType.LLLLVAR:
                    WriteLengthHeader(Length,
                        outs,
                        Type,
                        binary,
                        forceStringEncoding);
                    break;
                case IsoType.LLBIN:
                case IsoType.LLLBIN:
                case IsoType.LLLLBIN:
                    WriteLengthHeader(binary ? Length : Length * 2,
                        outs,
                        Type,
                        binary,
                        forceStringEncoding);
                    break;
                default:
                    if (binary)
                    {
                        //numeric types in binary are coded like this
                        sbyte[] buf = null;
                        switch (Type)
                        {
                            case IsoType.NUMERIC:
                                buf = new sbyte[Length / 2 + Length % 2];
                                break;
                            case IsoType.AMOUNT:
                                buf = new sbyte[6];
                                break;
                            case IsoType.DATE10:
                            case IsoType.DATE4:
                            case IsoType.DATE_EXP:
                            case IsoType.TIME:
                            case IsoType.DATE12:
                            case IsoType.DATE14:
                            case IsoType.DATE6:
                                buf = new sbyte[Length / 2];
                                break;
                        }

                        //Encode in BCD if it's one of these types
                        if (buf != null)
                        {
                            Bcd.Encode(ToString(),
                                buf);
                            outs.Write(buf.ToUnsignedBytes(),
                                0,
                                buf.Length);

                            return;
                        }
                    }

                    break;
            }

            if (binary && (Type == IsoType.BINARY || Type == IsoType.LLBIN || Type == IsoType.LLLBIN ||
                           Type == IsoType.LLLLBIN))
            {
                var missing = 0;
                if (Value is sbyte[] bytes)
                {
                    outs.Write(bytes.ToUnsignedBytes(),
                        0,
                        bytes.Length);

                    missing = Length - bytes.Length;
                }
                else if (Encoder is ICustomBinaryField customBinaryField)
                {
                    var binval = customBinaryField.EncodeBinaryField(Value);
                    outs.Write(binval.ToUnsignedBytes(),
                        0,
                        binval.Length);
                    missing = Length - binval.Length;
                }
                else
                {
                    var binval = HexCodec.HexDecode(Value.ToString());
                    outs.Write(binval.ToUnsignedBytes(),
                        0,
                        binval.Length);

                    missing = Length - binval.Length;
                }

                if (Type != IsoType.BINARY || missing <= 0) return;
                for (var i = 0; i < missing; i++) outs.WriteByte(0);
            }
            else
            {
                var bytes = ToString().GetSignedBytes(Encoding);
                outs.Write(bytes.ToUnsignedBytes(),
                    0,
                    bytes.Length);
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is IsoValue)) return false;
            var comp = (IsoValue) other;
            return comp.GetType() == GetType() && comp.Value.Equals(Value) && comp.Length == Length;
        }

        public override int GetHashCode() => Value == null ? 0 : ToString().GetHashCode();
    }
}