using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;

namespace NetCore8583.Codecs
{
    /// <summary>
    /// Codec for fields that contain multiple subfields (e.g. a concatenated set of ISO values). Parses and encodes using a list of <see cref="FieldParseInfo"/> parsers.
    /// </summary>
    public class CompositeField : ICustomBinaryField
    {
        private List<FieldParseInfo> parsers;

        /// <summary>The list of <see cref="IsoValue"/> subfield values when used as a decoded composite.</summary>
        public List<IsoValue> Values { get; set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object DecodeField(string value)
        {
            var vals = new List<IsoValue>(parsers.Count);
            var buf = value.GetSignedBytes();
            var pos = 0;
            try
            {
                foreach (var fpi in parsers)
                {
                    var v = fpi.Parse(
                        0,
                        buf,
                        pos,
                        fpi.Decoder);
                    if (v == null) continue;
                    pos += fpi.Encoding.GetBytes(v.ToString()).Length;
                    switch (v.Type)
                    {
                        case IsoType.LLVAR:
                        case IsoType.LLBIN:
                            pos += 2;
                            break;

                        case IsoType.LLLVAR:
                        case IsoType.LLLBIN:
                            pos += 3;
                            break;

                        case IsoType.LLLLBIN:
                        case IsoType.LLLLVAR:
                            pos += 4;
                            break;
                    }

                    vals.Add(v);
                }

                var f = new CompositeField
                {
                    Values = vals
                };
                return f;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public string EncodeField(object val)
        {
            try
            {
                var value = (CompositeField) val;
                Encoding encoding = null;
                var bout = new MemoryStream();
                foreach (var v in value.Values)
                {
                    v.Write(
                        bout,
                        false,
                        true);
                    if (encoding == null) encoding = v.Encoding;
                }

                var buf = bout.ToArray();
                return encoding == null ? Encoding.UTF8.GetString(buf) : encoding.GetString(buf);
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object DecodeBinaryField(sbyte[] buf,
            int offset,
            int length)
        {
            var vals = new List<IsoValue>(parsers.Count);
            var pos = offset;
            try
            {
                foreach (var fpi in parsers)
                {
                    var v = fpi.ParseBinary(0, buf, pos, fpi.Decoder);
                    if (v == null) continue;
                    
                    if (v.Type == IsoType.NUMERIC || v.Type == IsoType.DATE10 || v.Type == IsoType.DATE4 ||
                        v.Type == IsoType.DATE_EXP || v.Type == IsoType.AMOUNT || v.Type == IsoType.TIME ||
                        v.Type == IsoType.DATE12 || v.Type == IsoType.DATE14) 
                        pos += v.Length / 2 + v.Length % 2;
                    else 
                        pos += v.Length;
                        
                    switch (v.Type)
                    {
                        case IsoType.LLVAR:
                        case IsoType.LLBIN:
                            pos++;
                            break;

                        case IsoType.LLLVAR:
                        case IsoType.LLLBIN:
                        case IsoType.LLLLVAR:
                        case IsoType.LLLLBIN:
                            pos += 2;
                            break;
                    }

                    vals.Add(v);
                }

                var f = new CompositeField
                {
                    Values = vals
                };
                return f;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public sbyte[] EncodeBinaryField(object val)
        {
            var stream = new MemoryStream();
            try
            {
                var value = (CompositeField) val;
                foreach (var v in value.Values)
                    v.Write(
                        stream,
                        true,
                        true);
            }
            catch (IOException)
            {
                //shouldn't happen
            }

            var resultBytes = stream.ToArray();
            var resultSigned = new sbyte[resultBytes.Length];
            resultBytes.AsSignedBytes().CopyTo(resultSigned);
            return resultSigned;
        }

        /// <summary>Adds a subfield value to this composite. Used when building a composite for encoding.</summary>
        /// <param name="value">The <see cref="IsoValue"/> to add.</param>
        /// <returns>This composite (for chaining).</returns>
        public CompositeField AddValue(IsoValue value)
        {
            if (Values == null) Values = new List<IsoValue>(4);
            Values.Add(value);
            return this;
        }

        /// <summary>Adds a subfield value by creating an <see cref="IsoValue"/> from the given type, value, and optional encoder.</summary>
        /// <param name="val">The value to add.</param>
        /// <param name="encoder">Optional custom encoder.</param>
        /// <param name="t">The ISO type of the subfield.</param>
        /// <param name="length">Length for fixed-length types.</param>
        /// <returns>This composite (for chaining).</returns>
        public CompositeField AddValue(object val,
            ICustomField encoder,
            IsoType t,
            int length)
        {
            return AddValue(
                t.NeedsLength()
                    ? new IsoValue(
                        t,
                        val,
                        length,
                        encoder)
                    : new IsoValue(
                        t,
                        val,
                        encoder));
        }

        /// <summary>Returns the subfield value at the given index.</summary>
        /// <param name="idx">Zero-based index into <see cref="Values"/>.</param>
        /// <returns>The <see cref="IsoValue"/> at that index, or null if out of range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IsoValue GetField(int idx)
        {
            if (idx < 0 || idx >= Values.Count) return null;
            return Values[idx];
        }

        /// <summary>Returns the raw object value of the subfield at the given index.</summary>
        /// <param name="idx">Zero-based index into <see cref="Values"/>.</param>
        /// <returns>The <see cref="IsoValue.Value"/> at that index, or null if out of range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetObjectValue(int idx)
        {
            var v = GetField(idx);
            return v.Value;
        }

        /// <summary>Adds a parser used to decode this composite from message data. Order must match the subfield order.</summary>
        /// <param name="fpi">The <see cref="FieldParseInfo"/> for the next subfield.</param>
        /// <returns>This composite (for chaining).</returns>
        public CompositeField AddParser(FieldParseInfo fpi)
        {
            if (parsers == null) parsers = new List<FieldParseInfo>(4);
            parsers.Add(fpi);
            return this;
        }

        /// <summary>Returns the list of parsers used to decode this composite.</summary>
        /// <returns>The parser list (may be null if not yet configured).</returns>
        public List<FieldParseInfo> GetParsers()
        {
            return parsers;
        }

        /// <summary>Returns a string representation of this composite (e.g. "CompositeField[ALPHA,NUMERIC]").</summary>
        /// <returns>A string describing the composite and its subfield types.</returns>
        /// <summary>Returns a string representation of this composite (e.g. "CompositeField[ALPHA,NUMERIC]").</summary>
        public override string ToString()
        {
            if (Values == null) return "CompositeField[]";
            
            // Calculate required length
            var length = 16; // "CompositeField[]"
            var count = Values.Count;
            if (count > 0)
            {
                foreach (var v in Values)
                {
                    var typeName = v.Type.ToString();
                    length += typeName.Length;
                }
                length += count - 1; // commas
            }

            return string.Create(length, (Values, count), (span, state) =>
            {
                const string prefix = "CompositeField[";
                prefix.AsSpan().CopyTo(span);
                var pos = prefix.Length;
                
                for (var i = 0; i < state.count; i++)
                {
                    if (i > 0)
                    {
                        span[pos++] = ',';
                    }
                    
                    var typeName = state.Values[i].Type.ToString();
                    typeName.AsSpan().CopyTo(span.Slice(pos));
                    pos += typeName.Length;
                }
                
                span[pos] = ']';
            });
        }
    }
}