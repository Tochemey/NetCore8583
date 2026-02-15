using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore8583.Builder
{
    /// <summary>
    /// Fluent builder for defining ISO 8583 message templates.
    /// Supports setting field values, inheritance via <see cref="Extends"/>,
    /// and composite fields via <see cref="CompositeField"/>.
    /// </summary>
    public sealed class TemplateBuilder
    {
        internal int? ExtendsType;
        internal readonly List<TemplateFieldConfig> Fields = new();

        /// <summary>
        /// Specifies that this template inherits all fields from the given base message type.
        /// Inherited fields can be overridden by calling <see cref="Field(int, IsoType, string, int)"/>
        /// and related overloads.
        /// </summary>
        /// <param name="baseType">The message type to inherit from (e.g. 0x0200).</param>
        /// <returns>This builder for chaining.</returns>
        public TemplateBuilder Extends(int baseType)
        {
            ExtendsType = baseType;
            return this;
        }

        /// <summary>
        /// Adds a field with a string value for a fixed-length type (ALPHA, NUMERIC, BINARY).
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <param name="value">The default value.</param>
        /// <param name="length">The fixed length.</param>
        /// <returns>This builder for chaining.</returns>
        public TemplateBuilder Field(int num, IsoType type, string value, int length)
        {
            ValidateFieldNumber(num);
            Fields.Add(new TemplateFieldConfig(num, type, value, length, null, null));
            return this;
        }

        /// <summary>
        /// Adds a field with a string value for a variable-length or date/time type
        /// (LLVAR, LLLVAR, LLLLVAR, LLBIN, LLLBIN, LLLLBIN, DATE*, TIME, AMOUNT).
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <param name="value">The default value.</param>
        /// <returns>This builder for chaining.</returns>
        public TemplateBuilder Field(int num, IsoType type, string value)
        {
            ValidateFieldNumber(num);
            if (type.NeedsLength())
                throw new ArgumentException(
                    $"Type {type} requires a length; use the overload that accepts a length parameter.");
            Fields.Add(new TemplateFieldConfig(num, type, value, 0, null, null));
            return this;
        }

        /// <summary>
        /// Adds a field with a typed value and custom encoder for a fixed-length type.
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <param name="value">The default value.</param>
        /// <param name="length">The fixed length.</param>
        /// <param name="encoder">The custom encoder/decoder.</param>
        /// <returns>This builder for chaining.</returns>
        public TemplateBuilder Field(int num, IsoType type, object value, int length, ICustomField encoder)
        {
            ValidateFieldNumber(num);
            Fields.Add(new TemplateFieldConfig(num, type, value, length, encoder, null));
            return this;
        }

        /// <summary>
        /// Adds a field with a typed value and custom encoder for a variable-length type.
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <param name="value">The default value.</param>
        /// <param name="encoder">The custom encoder/decoder.</param>
        /// <returns>This builder for chaining.</returns>
        public TemplateBuilder Field(int num, IsoType type, object value, ICustomField encoder)
        {
            ValidateFieldNumber(num);
            if (type.NeedsLength())
                throw new ArgumentException(
                    $"Type {type} requires a length; use the overload that accepts a length parameter.");
            Fields.Add(new TemplateFieldConfig(num, type, value, 0, encoder, null));
            return this;
        }

        /// <summary>
        /// Adds a composite field for a fixed-length type, containing multiple subfield values.
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type (typically ALPHA, NUMERIC, or LLVAR/LLLVAR).</param>
        /// <param name="length">The total length for fixed-length types.</param>
        /// <param name="configure">An action to configure the composite subfields.</param>
        /// <returns>This builder for chaining.</returns>
        public TemplateBuilder CompositeField(int num, IsoType type, int length,
            Action<CompositeFieldBuilder> configure)
        {
            ValidateFieldNumber(num);
            var cfb = new CompositeFieldBuilder();
            configure(cfb);
            Fields.Add(new TemplateFieldConfig(num, type, null, length, null, cfb));
            return this;
        }

        /// <summary>
        /// Adds a composite field for a variable-length type, containing multiple subfield values.
        /// </summary>
        /// <param name="num">The field number (2–128).</param>
        /// <param name="type">The ISO type.</param>
        /// <param name="configure">An action to configure the composite subfields.</param>
        /// <returns>This builder for chaining.</returns>
        public TemplateBuilder CompositeField(int num, IsoType type,
            Action<CompositeFieldBuilder> configure)
        {
            ValidateFieldNumber(num);
            if (type.NeedsLength())
                throw new ArgumentException(
                    $"Type {type} requires a length; use the CompositeField overload that accepts a length parameter.");
            var cfb = new CompositeFieldBuilder();
            configure(cfb);
            Fields.Add(new TemplateFieldConfig(num, type, null, 0, null, cfb));
            return this;
        }

        /// <summary>
        /// Excludes (removes) a field inherited from the base template.
        /// Only meaningful when used with <see cref="Extends"/>.
        /// </summary>
        /// <param name="num">The field number to exclude.</param>
        /// <returns>This builder for chaining.</returns>
        public TemplateBuilder Exclude(int num)
        {
            ValidateFieldNumber(num);
            // Sentinel: type is ignored, value is null, length -1 signals exclusion
            Fields.Add(new TemplateFieldConfig(num, IsoType.ALPHA, null, -1, null, null));
            return this;
        }

        /// <summary>
        /// Builds the <see cref="IsoMessage"/> template from this builder's configuration.
        /// </summary>
        /// <param name="type">The message type (e.g. 0x0200).</param>
        /// <param name="encoding">The encoding to apply to all field values.</param>
        /// <param name="baseTemplate">
        /// The base template to inherit from (if <see cref="ExtendsType"/> was set);
        /// null if no inheritance.
        /// </param>
        /// <returns>A configured <see cref="IsoMessage"/> ready to be added as a template.</returns>
        internal IsoMessage Build(int type, Encoding encoding, IsoMessage baseTemplate)
        {
            var m = new IsoMessage { Type = type, Encoding = encoding };

            // Copy fields from the base template if extending
            if (baseTemplate != null)
            {
                for (var i = 2; i <= 128; i++)
                {
                    if (baseTemplate.HasField(i))
                        m.SetField(i, (IsoValue) baseTemplate.GetField(i).Clone());
                }
            }

            // Apply this builder's field configurations
            foreach (var fc in Fields)
            {
                // Exclusion sentinel
                if (fc.Length == -1 && fc.Value == null && fc.Composite == null)
                {
                    m.SetField(fc.Num, null);
                    continue;
                }

                IsoValue v;
                if (fc.Composite != null)
                {
                    // Build a composite field
                    var cf = fc.Composite.BuildValues(encoding);
                    v = fc.Type.NeedsLength()
                        ? new IsoValue(fc.Type, cf, fc.Length, cf)
                        : new IsoValue(fc.Type, cf, cf);
                }
                else if (fc.Encoder != null)
                {
                    v = fc.Type.NeedsLength()
                        ? new IsoValue(fc.Type, fc.Encoder.DecodeField(fc.Value?.ToString()), fc.Length, fc.Encoder)
                        : new IsoValue(fc.Type, fc.Encoder.DecodeField(fc.Value?.ToString()), fc.Encoder);
                }
                else
                {
                    v = fc.Type.NeedsLength()
                        ? new IsoValue(fc.Type, fc.Value ?? string.Empty, fc.Length)
                        : new IsoValue(fc.Type, fc.Value ?? string.Empty);
                }

                v.Encoding = encoding;
                m.SetField(fc.Num, v);
            }

            return m;
        }

        private static void ValidateFieldNumber(int num)
        {
            if (num is < 2 or > 128)
                throw new ArgumentOutOfRangeException(nameof(num), num,
                    "Field number must be between 2 and 128.");
        }

        /// <summary>Configuration record for a single template field.</summary>
        internal readonly struct TemplateFieldConfig
        {
            public readonly int Num;
            public readonly IsoType Type;
            public readonly object Value;
            public readonly int Length;
            public readonly ICustomField Encoder;
            public readonly CompositeFieldBuilder Composite;

            public TemplateFieldConfig(int num, IsoType type, object value, int length,
                ICustomField encoder, CompositeFieldBuilder composite)
            {
                Num = num;
                Type = type;
                Value = value;
                Length = length;
                Encoder = encoder;
                Composite = composite;
            }
        }
    }
}
