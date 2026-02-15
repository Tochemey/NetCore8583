using System;
using System.Collections.Generic;
using System.Text;
using NetCore8583.Parse;

namespace NetCore8583.Builder
{
    /// <summary>
    /// Fluent builder for defining ISO 8583 parse maps (parsing guides).
    /// Supports field definitions, inheritance via <see cref="Extends"/>,
    /// field exclusion, and composite fields.
    /// </summary>
    public sealed class ParseMapBuilder
    {
        internal int? ExtendsType;
        internal readonly List<ParseFieldConfig> Fields = new();
        internal readonly HashSet<int> Excludes = new();

        /// <summary>
        /// Specifies that this parse map inherits all field parsers from the given base message type.
        /// Inherited fields can be overridden or excluded.
        /// </summary>
        /// <param name="baseType">The message type to inherit from (e.g. 0x0200).</param>
        /// <returns>This builder for chaining.</returns>
        public ParseMapBuilder Extends(int baseType)
        {
            ExtendsType = baseType;
            return this;
        }

        /// <summary>
        /// Adds a field parser for a fixed-length type (ALPHA, NUMERIC, BINARY).
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <param name="length">The fixed length.</param>
        /// <returns>This builder for chaining.</returns>
        public ParseMapBuilder Field(int num, IsoType type, int length)
        {
            ValidateFieldNumber(num);
            Fields.Add(new ParseFieldConfig(num, type, length, null));
            return this;
        }

        /// <summary>
        /// Adds a field parser for a variable-length or date/time type
        /// (LLVAR, LLLVAR, LLLLVAR, LLBIN, LLLBIN, LLLLBIN, DATE*, TIME, AMOUNT).
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <returns>This builder for chaining.</returns>
        public ParseMapBuilder Field(int num, IsoType type)
        {
            ValidateFieldNumber(num);
            if (type.NeedsLength())
                throw new ArgumentException(
                    $"Type {type} requires a length; use the overload that accepts a length parameter.");
            Fields.Add(new ParseFieldConfig(num, type, 0, null));
            return this;
        }

        /// <summary>
        /// Adds a composite field parser for a fixed-length type, containing multiple subfield parsers.
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <param name="length">The total length for fixed-length types.</param>
        /// <param name="configure">An action to configure the composite subfield parsers.</param>
        /// <returns>This builder for chaining.</returns>
        public ParseMapBuilder CompositeField(int num, IsoType type, int length,
            Action<CompositeFieldBuilder> configure)
        {
            ValidateFieldNumber(num);
            var cfb = new CompositeFieldBuilder();
            configure(cfb);
            Fields.Add(new ParseFieldConfig(num, type, length, cfb));
            return this;
        }

        /// <summary>
        /// Adds a composite field parser for a variable-length type, containing multiple subfield parsers.
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <param name="configure">An action to configure the composite subfield parsers.</param>
        /// <returns>This builder for chaining.</returns>
        public ParseMapBuilder CompositeField(int num, IsoType type,
            Action<CompositeFieldBuilder> configure)
        {
            ValidateFieldNumber(num);
            if (type.NeedsLength())
                throw new ArgumentException(
                    $"Type {type} requires a length; use the CompositeField overload that accepts a length parameter.");
            var cfb = new CompositeFieldBuilder();
            configure(cfb);
            Fields.Add(new ParseFieldConfig(num, type, 0, cfb));
            return this;
        }

        /// <summary>
        /// Excludes (removes) a field from the inherited parse map.
        /// Only meaningful when used with <see cref="Extends"/>.
        /// </summary>
        /// <param name="num">The field number to exclude.</param>
        /// <returns>This builder for chaining.</returns>
        public ParseMapBuilder Exclude(int num)
        {
            ValidateFieldNumber(num);
            Excludes.Add(num);
            return this;
        }

        /// <summary>
        /// Builds a <see cref="Dictionary{TKey,TValue}"/> of field number to <see cref="FieldParseInfo"/>
        /// from this builder's configuration.
        /// </summary>
        /// <param name="encoding">The encoding to set on each field parser.</param>
        /// <param name="baseParseMap">
        /// The base parse map to inherit from (if <see cref="ExtendsType"/> was set);
        /// null if no inheritance.
        /// </param>
        /// <returns>A dictionary suitable for passing to <see cref="MessageFactory{T}.SetParseMap"/>.</returns>
        internal Dictionary<int, FieldParseInfo> Build(Encoding encoding,
            Dictionary<int, FieldParseInfo> baseParseMap)
        {
            var map = new Dictionary<int, FieldParseInfo>();

            // Copy from base if extending
            if (baseParseMap != null)
            {
                foreach (var kvp in baseParseMap)
                    map[kvp.Key] = kvp.Value;
            }

            // Apply exclusions
            foreach (var num in Excludes)
                map.Remove(num);

            // Apply/override fields
            foreach (var fc in Fields)
            {
                FieldParseInfo fpi;
                if (fc.Composite != null)
                {
                    fpi = FieldParseInfo.GetInstance(fc.Type, fc.Length, encoding);
                    fpi.Decoder = fc.Composite.BuildParsers(encoding);
                }
                else
                {
                    fpi = FieldParseInfo.GetInstance(fc.Type, fc.Length, encoding);
                }

                map[fc.Num] = fpi;
            }

            return map;
        }

        private static void ValidateFieldNumber(int num)
        {
            if (num is < 2 or > 128)
                throw new ArgumentOutOfRangeException(nameof(num), num,
                    "Field number must be between 2 and 128.");
        }

        /// <summary>Configuration record for a single parse field.</summary>
        internal readonly struct ParseFieldConfig
        {
            public readonly int Num;
            public readonly IsoType Type;
            public readonly int Length;
            public readonly CompositeFieldBuilder Composite;

            public ParseFieldConfig(int num, IsoType type, int length, CompositeFieldBuilder composite)
            {
                Num = num;
                Type = type;
                Length = length;
                Composite = composite;
            }
        }
    }
}
