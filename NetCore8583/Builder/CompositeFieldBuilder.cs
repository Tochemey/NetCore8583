using System;
using System.Collections.Generic;
using System.Text;
using NetCore8583.Codecs;
using NetCore8583.Parse;

namespace NetCore8583.Builder
{
    /// <summary>
    /// Fluent builder for constructing <see cref="CompositeField"/> instances, supporting both
    /// template subfields (with values) and parse-map subfield parsers.
    /// </summary>
    public sealed class CompositeFieldBuilder
    {
        internal readonly List<SubFieldConfig> SubFields = new();
        internal readonly List<SubParserConfig> SubParsers = new();

        /// <summary>
        /// Adds a subfield value for a fixed-length type (ALPHA, NUMERIC, BINARY).
        /// Used when building template composite fields.
        /// </summary>
        /// <param name="type">The ISO type of the subfield.</param>
        /// <param name="value">The subfield value.</param>
        /// <param name="length">The fixed length.</param>
        /// <returns>This builder for chaining.</returns>
        public CompositeFieldBuilder SubField(IsoType type, string value, int length)
        {
            SubFields.Add(new SubFieldConfig(type, value, length, null));
            return this;
        }

        /// <summary>
        /// Adds a subfield value for a variable-length or date/time type (LLVAR, LLLVAR, etc.).
        /// Used when building template composite fields.
        /// </summary>
        /// <param name="type">The ISO type of the subfield.</param>
        /// <param name="value">The subfield value.</param>
        /// <returns>This builder for chaining.</returns>
        public CompositeFieldBuilder SubField(IsoType type, string value)
        {
            if (type.NeedsLength())
                throw new ArgumentException(
                    $"Type {type} requires a length; use the overload that accepts a length parameter.");
            SubFields.Add(new SubFieldConfig(type, value, 0, null));
            return this;
        }

        /// <summary>
        /// Adds a subfield value with a custom encoder for a fixed-length type.
        /// Used when building template composite fields.
        /// </summary>
        /// <param name="type">The ISO type of the subfield.</param>
        /// <param name="value">The subfield value.</param>
        /// <param name="length">The fixed length.</param>
        /// <param name="encoder">The custom encoder/decoder.</param>
        /// <returns>This builder for chaining.</returns>
        public CompositeFieldBuilder SubField(IsoType type, object value, int length, ICustomField encoder)
        {
            SubFields.Add(new SubFieldConfig(type, value, length, encoder));
            return this;
        }

        /// <summary>
        /// Adds a subfield value with a custom encoder for a variable-length type.
        /// Used when building template composite fields.
        /// </summary>
        /// <param name="type">The ISO type of the subfield.</param>
        /// <param name="value">The subfield value.</param>
        /// <param name="encoder">The custom encoder/decoder.</param>
        /// <returns>This builder for chaining.</returns>
        public CompositeFieldBuilder SubField(IsoType type, object value, ICustomField encoder)
        {
            if (type.NeedsLength())
                throw new ArgumentException(
                    $"Type {type} requires a length; use the overload that accepts a length parameter.");
            SubFields.Add(new SubFieldConfig(type, value, 0, encoder));
            return this;
        }

        /// <summary>
        /// Adds a subfield parser for a fixed-length type (ALPHA, NUMERIC, BINARY).
        /// Used when building parse-map composite fields.
        /// </summary>
        /// <param name="type">The ISO type of the subfield parser.</param>
        /// <param name="length">The fixed length.</param>
        /// <returns>This builder for chaining.</returns>
        public CompositeFieldBuilder SubParser(IsoType type, int length)
        {
            SubParsers.Add(new SubParserConfig(type, length));
            return this;
        }

        /// <summary>
        /// Adds a subfield parser for a variable-length or date/time type.
        /// Used when building parse-map composite fields.
        /// </summary>
        /// <param name="type">The ISO type of the subfield parser.</param>
        /// <returns>This builder for chaining.</returns>
        public CompositeFieldBuilder SubParser(IsoType type)
        {
            if (type.NeedsLength())
                throw new ArgumentException(
                    $"Type {type} requires a length; use the overload that accepts a length parameter.");
            SubParsers.Add(new SubParserConfig(type, 0));
            return this;
        }

        /// <summary>
        /// Builds a <see cref="CompositeField"/> with the configured subfield values (for templates).
        /// </summary>
        /// <param name="encoding">The encoding to set on each subfield value.</param>
        /// <returns>A populated <see cref="CompositeField"/>.</returns>
        internal CompositeField BuildValues(Encoding encoding)
        {
            var cf = new CompositeField();
            foreach (var sf in SubFields)
            {
                IsoValue v;
                if (sf.Type.NeedsLength())
                    v = new IsoValue(sf.Type, sf.Value, sf.Length, sf.Encoder);
                else
                    v = new IsoValue(sf.Type, sf.Value, sf.Encoder);

                v.Encoding = encoding;
                cf.AddValue(v);
            }

            return cf;
        }

        /// <summary>
        /// Builds a <see cref="CompositeField"/> with the configured subfield parsers (for parse maps).
        /// </summary>
        /// <param name="encoding">The encoding to set on each parser.</param>
        /// <returns>A populated <see cref="CompositeField"/> with parsers.</returns>
        internal CompositeField BuildParsers(Encoding encoding)
        {
            var cf = new CompositeField();
            foreach (var sp in SubParsers)
            {
                var fpi = FieldParseInfo.GetInstance(sp.Type, sp.Length, encoding);
                cf.AddParser(fpi);
            }

            return cf;
        }

        /// <summary>Configuration for a single subfield value in a composite template field.</summary>
        internal readonly struct SubFieldConfig
        {
            public readonly IsoType Type;
            public readonly object Value;
            public readonly int Length;
            public readonly ICustomField Encoder;

            public SubFieldConfig(IsoType type, object value, int length, ICustomField encoder)
            {
                Type = type;
                Value = value;
                Length = length;
                Encoder = encoder;
            }
        }

        /// <summary>Configuration for a single subfield parser in a composite parse field.</summary>
        internal readonly struct SubParserConfig
        {
            public readonly IsoType Type;
            public readonly int Length;

            public SubParserConfig(IsoType type, int length)
            {
                Type = type;
                Length = length;
            }
        }
    }
}
