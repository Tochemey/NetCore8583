using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using NetCore8583.Extensions;

namespace NetCore8583
{
    /// <summary>
    /// Holds a single ISO 8583 field value with its type, length, optional custom encoder, and encoding. Used both when building messages and when parsing.
    /// </summary>
    public class IsoValue : ICloneable
    {
        /// <summary>Creates an IsoValue for variable-length types (LLVAR, LLLVAR, LLLLVAR, LLBIN, LLLBIN, LLLLBIN). Length is derived from the value.</summary>
        /// <param name="t">The ISO type (must not be ALPHA, NUMERIC, or BINARY).</param>
        /// <param name="value">The field value (string, sbyte[], or custom object).</param>
        /// <param name="custom">Optional custom encoder/decoder.</param>
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

                    switch (t)
                    {
                        case IsoType.LLVAR when Length > 99:
                            throw new ArgumentException("LLVAR can only hold values up to 99 chars");
                        case IsoType.LLLVAR when Length > 999:
                            throw new ArgumentException("LLLVAR can only hold values up to 999 chars");
                        case IsoType.LLLLVAR when Length > 9999:
                            throw new ArgumentException("LLLLVAR can only hold values up to 9999 chars");
                    }

                    break;
                case IsoType.LLBIN:
                case IsoType.LLLBIN:
                case IsoType.LLLLBIN:
                    switch (Encoder)
                    {
                        case null when value.GetType() == typeof(sbyte[]):
                        {
                            var obj = value;
                            Length = ((sbyte[]) obj).Length;
                            break;
                        }
                        case null:
                            Length = value.ToString().Length / 2 + value.ToString().Length % 2;
                            break;
                        case ICustomBinaryField binaryField:
                            Length = binaryField.EncodeBinaryField(value).Length;
                            break;
                        default:
                        {
                            var enc = Encoder.EncodeField(value) ?? (value?.ToString() ?? string.Empty);
                            Length = enc.Length;
                            break;
                        }
                    }

                    switch (t)
                    {
                        case IsoType.LLBIN when Length > 99:
                            throw new ArgumentException("LLBIN can only hold values up to 99 chars");
                        case IsoType.LLLBIN when Length > 999:
                            throw new ArgumentException("LLLBIN can only hold values up to 999 chars");
                        case IsoType.LLLLBIN when Length > 9999:
                            throw new ArgumentException("LLLLBIN can only hold values up to 9999 chars");
                    }

                    break;
                default:
                    Length = Type.Length();
                    break;
            }
        }


        /// <summary>Creates an IsoValue with explicit length. Use for fixed-length types (ALPHA, NUMERIC, BINARY) or when length is known for variable-length types.</summary>
        /// <param name="t">The ISO type.</param>
        /// <param name="val">The field value.</param>
        /// <param name="len">The length (required for fixed-length types).</param>
        /// <param name="custom">Optional custom encoder/decoder.</param>
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
                    switch (t)
                    {
                        case IsoType.LLVAR when Length > 99:
                            throw new ArgumentException("LLVAR can only hold values up to 99 chars");
                        case IsoType.LLLVAR when Length > 999:
                            throw new ArgumentException("LLLVAR can only hold values up to 999 chars");
                        case IsoType.LLLLVAR when Length > 9999:
                            throw new ArgumentException("LLLLVAR can only hold values up to 9999 chars");
                    }

                    break;
                case IsoType.LLBIN:
                case IsoType.LLLBIN:
                case IsoType.LLLLBIN:
                    if (len == 0)
                    {
                        switch (Encoder)
                        {
                            case null:
                            {
                                var obj = val;
                                Length = ((byte[]) obj).Length;
                                break;
                            }
                            case ICustomBinaryField _:
                            {
                                var customBinaryField = (ICustomBinaryField) custom;
                                if (customBinaryField != null) Length = customBinaryField.EncodeBinaryField(Value).Length;
                                break;
                            }
                            default:
                                Length = Encoder.EncodeField(Value).Length;
                                break;
                        }

                        Length = Encoder?.EncodeField(Value).Length ?? ((sbyte[]) val).Length;
                    }

                    switch (t)
                    {
                        case IsoType.LLBIN when Length > 99:
                            throw new ArgumentException("LLBIN can only hold values up to 99 chars");
                        case IsoType.LLLBIN when Length > 999:
                            throw new ArgumentException("LLLBIN can only hold values up to 999 chars");
                        case IsoType.LLLLBIN when Length > 9999:
                            throw new ArgumentException("LLLLBIN can only hold values up to 9999 chars");
                    }

                    break;
            }
        }

        /// <summary>Optional custom encoder/decoder for this field.</summary>
        public ICustomField Encoder { get; }

        /// <summary>The ISO type of this field.</summary>
        public IsoType Type { get; }

        /// <summary>The length of the field (fixed or computed for variable-length types).</summary>
        public int Length { get; }

        /// <summary>Character encoding for string representation. Default is <see cref="Encoding.Default"/>.</summary>
        public Encoding Encoding { get; set; } = Encoding.Default;

        /// <summary>The stored value (string, sbyte[], decimal, DateTime, or custom type).</summary>
        public object Value { get; }

        /// <summary>Creates a shallow copy of this IsoValue.</summary>
        /// <returns>A new IsoValue with the same type, value, length, and encoder.</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>Returns the encoded string representation of the value for writing to the message (e.g. formatted numeric, hex for binary).</summary>
        /// <returns>Encoded string for this field.</returns>
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
                    else
                        return Value switch
                        {
                            BigInteger _ => Type.Format(Encoder == null ? Value.ToString() : Encoder.EncodeField(Value),
                                Length),
                            long l => Type.Format(l, Length),
                            _ => Type.Format(Encoder == null ? Value.ToString() : Encoder.EncodeField(Value), Length)
                        };
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

        /// <summary>
        /// Writes the length header for variable-length fields (LLVAR, LLLVAR, LLLLVAR, LLBIN, LLLBIN, LLLLBIN) in ASCII or BCD as appropriate.
        /// </summary>
        /// <param name="l">The length value to write.</param>
        /// <param name="outs">The output stream.</param>
        /// <param name="type">The ISO type (determines number of digits: 2, 3, or 4).</param>
        /// <param name="binary">True to write BCD-encoded length; false for ASCII digits.</param>
        /// <param name="forceStringEncoding">When true, length is encoded as string using current encoding.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteLengthHeader(int l,
            Stream outs,
            IsoType type,
            bool binary,
            bool forceStringEncoding)
        {
            Span<sbyte> buffer = stackalloc sbyte[4];
            int position = 0;
            
            var digits = type switch
            {
                IsoType.LLLLBIN => 4,
                IsoType.LLLLVAR => 4,
                IsoType.LLLBIN => 3,
                IsoType.LLLVAR => 3,
                _ => 2
            };

            if (binary)
            {
                switch (digits)
                {
                    case 4:
                        buffer[position++] = (sbyte) (((l % 10000 / 1000) << 4) | (l % 1000 / 100));
                        break;
                    case 3:
                        buffer[position++] = (sbyte) (l / 100); //00 to 09 automatically in BCD
                        break;
                }

                //BCD encode the rest of the length
                buffer[position++] = (sbyte) (((l % 100 / 10) << 4) | (l % 10));
            }
            else if (forceStringEncoding)
            {
                Span<char> charBuffer = stackalloc char[4];
                l.TryFormat(charBuffer, out int charsWritten);
                
                // Pad with zeros
                var ldiff = digits - charsWritten;
                if (ldiff > 0)
                {
                    for (int i = charsWritten - 1; i >= 0; i--)
                    {
                        charBuffer[i + ldiff] = charBuffer[i];
                    }
                    for (int i = 0; i < ldiff; i++)
                    {
                        charBuffer[i] = '0';
                    }
                    charsWritten = digits;
                }

                position = new string(charBuffer.Slice(0, charsWritten)).GetSignedBytes(buffer, Encoding);
            }
            else
            {
                //write the length in ASCII
                switch (digits)
                {
                    case 4:
                        buffer[position++] = (sbyte) (l / 1000 + 48);
                        buffer[position++] = (sbyte) (l % 1000 / 100 + 48);
                        break;
                    case 3:
                        buffer[position++] = (sbyte) (l / 100 + 48);
                        break;
                }

                if (l >= 10) 
                    buffer[position++] = (sbyte) (l % 100 / 10 + 48);
                else 
                    buffer[position++] = 48;
                    
                buffer[position++] = (sbyte) (l % 10 + 48);
            }

            var byteSpan = buffer.Slice(0, position).AsUnsignedBytes();
            outs.Write(byteSpan.ToArray(), 0, byteSpan.Length);
        }

        /// <summary>
        /// Writes this field's value to the output stream (including length header for variable-length types), using binary or ASCII encoding as specified.
        /// </summary>
        /// <param name="outs">The output stream.</param>
        /// <param name="binary">True for binary encoding (BCD for numerics/dates, raw bytes for binary types).</param>
        /// <param name="forceStringEncoding">When true, use string encoding for length headers and compatible fields.</param>
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
                        var buf = Type switch
                        {
                            IsoType.NUMERIC => new sbyte[Length / 2 + Length % 2],
                            IsoType.AMOUNT => new sbyte[6],
                            IsoType.DATE10 => new sbyte[Length / 2],
                            IsoType.DATE4 => new sbyte[Length / 2],
                            IsoType.DATE_EXP => new sbyte[Length / 2],
                            IsoType.TIME => new sbyte[Length / 2],
                            IsoType.DATE12 => new sbyte[Length / 2],
                            IsoType.DATE14 => new sbyte[Length / 2],
                            IsoType.DATE6 => new sbyte[Length / 2],
                            _ => null
                        };

                        //Encode in BCD if it's one of these types
                        if (buf != null)
                        {
                            Bcd.Encode(ToString(),
                                buf);
                            var unsignedBuf = buf.AsUnsignedBytes();
                            outs.Write(unsignedBuf.ToArray(), 0, unsignedBuf.Length);
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
                    var unsignedBytes = bytes.AsUnsignedBytes();
                    outs.Write(unsignedBytes.ToArray(), 0, unsignedBytes.Length);
                    missing = Length - bytes.Length;
                }
                else switch (Encoder)
                {
                    case ICustomBinaryField customBinaryField:
                    {
                        var binval = customBinaryField.EncodeBinaryField(Value);
                        var unsignedBinval = binval.AsUnsignedBytes();
                        outs.Write(unsignedBinval.ToArray(), 0, unsignedBinval.Length);
                        missing = Length - binval.Length;
                        break;
                    }
                    default:
                    {
                        var binval = HexCodec.HexDecode(Value.ToString());
                        var unsignedBinval = binval.AsUnsignedBytes();
                        outs.Write(unsignedBinval.ToArray(), 0, unsignedBinval.Length);
                        missing = Length - binval.Length;
                        break;
                    }
                }

                if (Type != IsoType.BINARY || missing <= 0) return;
                for (var i = 0; i < missing; i++) outs.WriteByte(0);
            }
            else
            {
                var signedBytes = ToString().GetSignedBytes(Encoding);
                var unsignedBytes = signedBytes.AsUnsignedBytes();
                outs.Write(unsignedBytes.ToArray(), 0, unsignedBytes.Length);
            }
        }

        /// <summary>Compares this IsoValue to another by type, value, and length.</summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>True if the other object is an IsoValue with the same type, value, and length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (!(other is IsoValue)) return false;
            var comp = (IsoValue) other;
            return comp.GetType() == GetType() && comp.Value.Equals(Value) && comp.Length == Length;
        }

        /// <summary>Returns a hash code based on the string representation of the value.</summary>
        /// <returns>Hash code for this IsoValue.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Value == null ? 0 : ToString().GetHashCode();
    }
}