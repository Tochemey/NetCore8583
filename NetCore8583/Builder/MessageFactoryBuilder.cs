using System;
using System.Collections.Generic;
using System.Text;
using NetCore8583.Parse;

namespace NetCore8583.Builder
{
    /// <summary>
    /// Fluent builder for constructing a fully-configured <see cref="MessageFactory{T}"/> without XML.
    /// <para>
    /// Supports all configuration options available through XML: headers, templates with inheritance,
    /// parse maps with inheritance and exclusion, custom fields, composite fields, and all factory properties.
    /// </para>
    /// <example>
    /// <code>
    /// var factory = new MessageFactoryBuilder&lt;IsoMessage&gt;()
    ///     .WithEncoding(Encoding.UTF8)
    ///     .WithHeader(0x0200, "ISO")
    ///     .WithTemplate(0x0200, t => t
    ///         .Field(3, IsoType.NUMERIC, "000000", 6)
    ///         .Field(11, IsoType.NUMERIC, "000001", 6)
    ///         .Field(41, IsoType.ALPHA, "TERMINAL", 8))
    ///     .WithTemplate(0x0210, t => t
    ///         .Extends(0x0200)
    ///         .Field(39, IsoType.ALPHA, "00", 2))
    ///     .WithParseMap(0x0200, p => p
    ///         .Field(3, IsoType.NUMERIC, 6)
    ///         .Field(11, IsoType.NUMERIC, 6)
    ///         .Field(41, IsoType.ALPHA, 8))
    ///     .WithParseMap(0x0210, p => p
    ///         .Extends(0x0200)
    ///         .Field(39, IsoType.ALPHA, 2))
    ///     .Build();
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">The message type, must be or derive from <see cref="IsoMessage"/>.</typeparam>
    public sealed class MessageFactoryBuilder<T> where T : IsoMessage
    {
        private Encoding _encoding = Encoding.Default;
        private bool _forceStringEncoding;
        private int _radix = 10;
        private bool _useBinaryMessages;
        private bool _useBinaryBitmap;
        private bool _enforceSecondBitmap;
        private int _etx = -1;
        private bool _ignoreLast;
        private bool _assignDate;
        private ITraceNumberGenerator _traceGenerator;

        private readonly Dictionary<int, string> _isoHeaders = new();
        private readonly Dictionary<int, byte[]> _binaryIsoHeaders = new();
        private readonly Dictionary<int, ICustomField> _customFields = new();

        // Ordered lists to preserve declaration order (important for extends)
        private readonly List<(int Type, TemplateBuilder Builder)> _templates = new();
        private readonly List<(int Type, ParseMapBuilder Builder)> _parseMaps = new();

        // ──────────────────────────────────────────────────────────────────────
        //  Factory properties
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Sets the character encoding for the factory, templates, and parsers.</summary>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithEncoding(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            return this;
        }

        /// <summary>Enables or disables forced string encoding for variable-length length headers.</summary>
        /// <param name="value">True to force string encoding; false otherwise.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithForceStringEncoding(bool value = true)
        {
            _forceStringEncoding = value;
            return this;
        }

        /// <summary>Sets the radix used for decoding length headers (e.g. 10 for decimal).</summary>
        /// <param name="radix">The radix value.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithRadix(int radix)
        {
            _radix = radix;
            return this;
        }

        /// <summary>Enables or disables binary message encoding.</summary>
        /// <param name="value">True to use binary messages; false for ASCII.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithBinaryMessages(bool value = true)
        {
            _useBinaryMessages = value;
            return this;
        }

        /// <summary>Enables or disables binary bitmap encoding.</summary>
        /// <param name="value">True to use binary bitmaps; false for ASCII hex.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithBinaryBitmap(bool value = true)
        {
            _useBinaryBitmap = value;
            return this;
        }

        /// <summary>Enables or disables enforcing the secondary bitmap even when no fields 65–128 are set.</summary>
        /// <param name="value">True to enforce; false otherwise.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithEnforceSecondBitmap(bool value = true)
        {
            _enforceSecondBitmap = value;
            return this;
        }

        /// <summary>Sets the ETX (end-of-text) character to append to messages. Use -1 for none (default).</summary>
        /// <param name="etx">The ETX character value, or -1 for none.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithEtx(int etx)
        {
            _etx = etx;
            return this;
        }

        /// <summary>Sets whether the last byte of incoming messages should be ignored (e.g. ETX).</summary>
        /// <param name="value">True to ignore the last byte; false otherwise.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithIgnoreLast(bool value = true)
        {
            _ignoreLast = value;
            return this;
        }

        /// <summary>Enables or disables automatic assignment of the current date to field 7 on new messages.</summary>
        /// <param name="value">True to auto-assign; false otherwise.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithAssignDate(bool value = true)
        {
            _assignDate = value;
            return this;
        }

        /// <summary>Sets the trace number generator for field 11 on new messages.</summary>
        /// <param name="generator">The trace number generator.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithTraceGenerator(ITraceNumberGenerator generator)
        {
            _traceGenerator = generator;
            return this;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Headers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Sets an ASCII ISO header for the specified message type.</summary>
        /// <param name="type">The message type (e.g. 0x0200).</param>
        /// <param name="header">The ISO header string.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithHeader(int type, string header)
        {
            _isoHeaders[type] = header ?? throw new ArgumentNullException(nameof(header));
            _binaryIsoHeaders.Remove(type);
            return this;
        }

        /// <summary>
        /// Sets an ASCII ISO header for a message type, referencing the same header already
        /// configured for another message type. The referenced header must be configured
        /// before the referencing one.
        /// </summary>
        /// <param name="type">The message type to set the header for.</param>
        /// <param name="refType">The message type whose header to reuse.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithHeaderRef(int type, int refType)
        {
            if (!_isoHeaders.TryGetValue(refType, out var header))
                throw new ArgumentException(
                    $"Referenced header for type 0x{refType:X4} does not exist. " +
                    "Make sure the referenced header is configured before the referencing one.");
            _isoHeaders[type] = header;
            _binaryIsoHeaders.Remove(type);
            return this;
        }

        /// <summary>Sets a binary ISO header for the specified message type.</summary>
        /// <param name="type">The message type (e.g. 0x0200).</param>
        /// <param name="header">The binary header bytes.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithBinaryHeader(int type, byte[] header)
        {
            _binaryIsoHeaders[type] = header ?? throw new ArgumentNullException(nameof(header));
            _isoHeaders.Remove(type);
            return this;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Custom fields
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Registers a custom encoder/decoder for the specified field number.</summary>
        /// <param name="fieldNumber">The field number (2–128).</param>
        /// <param name="customField">The custom field codec.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithCustomField(int fieldNumber, ICustomField customField)
        {
            _customFields[fieldNumber] = customField ?? throw new ArgumentNullException(nameof(customField));
            return this;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Templates
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a message template for the specified type.
        /// Use the <paramref name="configure"/> action to define fields, inheritance, and composite fields.
        /// </summary>
        /// <param name="type">The message type (e.g. 0x0200).</param>
        /// <param name="configure">An action to configure the template's fields.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithTemplate(int type, Action<TemplateBuilder> configure)
        {
            var builder = new TemplateBuilder();
            configure(builder);
            _templates.Add((type, builder));
            return this;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Parse maps
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a parse map (parsing guide) for the specified message type.
        /// Use the <paramref name="configure"/> action to define field parsers, inheritance, and exclusions.
        /// </summary>
        /// <param name="type">The message type (e.g. 0x0200).</param>
        /// <param name="configure">An action to configure the parse map's field parsers.</param>
        /// <returns>This builder for chaining.</returns>
        public MessageFactoryBuilder<T> WithParseMap(int type, Action<ParseMapBuilder> configure)
        {
            var builder = new ParseMapBuilder();
            configure(builder);
            _parseMaps.Add((type, builder));
            return this;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Build
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds and returns a fully configured <see cref="MessageFactory{T}"/> from the current
        /// builder state.
        /// </summary>
        /// <returns>A new <see cref="MessageFactory{T}"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when a template or parse map references a nonexistent base type via <c>Extends</c>.
        /// </exception>
        public MessageFactory<T> Build()
        {
            var factory = new MessageFactory<T>();

            // 1. Set encoding early so FieldParseInfo and templates get the right encoding
            factory.Encoding = _encoding;

            // 2. Register custom fields (needed by template building if custom decoders are used)
            foreach (var (fieldNumber, customField) in _customFields)
                factory.SetCustomField(fieldNumber, customField);

            // 3. Set headers
            foreach (var (type, header) in _isoHeaders)
                factory.SetIsoHeader(type, header);
            foreach (var (type, header) in _binaryIsoHeaders)
                factory.SetBinaryIsoHeader(type, header);

            // 4. Build and add templates (in order, so extends works)
            var builtTemplates = new Dictionary<int, IsoMessage>();
            foreach (var (type, builder) in _templates)
            {
                IsoMessage baseTemplate = null;
                if (builder.ExtendsType.HasValue)
                {
                    if (!builtTemplates.TryGetValue(builder.ExtendsType.Value, out baseTemplate))
                        throw new ArgumentException(
                            $"Template for type 0x{type:X4} extends nonexistent template 0x{builder.ExtendsType.Value:X4}. " +
                            "Make sure the base template is defined before the extending one.");
                }

                var template = builder.Build(type, _encoding, baseTemplate);
                builtTemplates[type] = template;
                factory.AddMessageTemplate((T) template);
            }

            // 5. Build and set parse maps (in order, so extends works)
            var builtParseMaps = new Dictionary<int, Dictionary<int, FieldParseInfo>>();
            foreach (var (type, builder) in _parseMaps)
            {
                Dictionary<int, FieldParseInfo> baseMap = null;
                if (builder.ExtendsType.HasValue)
                {
                    if (!builtParseMaps.TryGetValue(builder.ExtendsType.Value, out baseMap))
                        throw new ArgumentException(
                            $"Parse map for type 0x{type:X4} extends nonexistent parse map 0x{builder.ExtendsType.Value:X4}. " +
                            "Make sure the base parse map is defined before the extending one.");
                }

                var parseMap = builder.Build(_encoding, baseMap);
                builtParseMaps[type] = parseMap;
                factory.SetParseMap(type, parseMap);
            }

            // 6. Set remaining factory properties
            factory.UseBinaryMessages = _useBinaryMessages;
            factory.UseBinaryBitmap = _useBinaryBitmap;
            factory.EnforceSecondBitmap = _enforceSecondBitmap;
            factory.Etx = _etx;
            factory.IgnoreLast = _ignoreLast;
            factory.AssignDate = _assignDate;
            factory.TraceGenerator = _traceGenerator;

            // 7. Re-set encoding properties to propagate to all parsers and templates
            //    (mirrors the pattern in MessageFactory.SetConfigPath)
            factory.Encoding = _encoding;
            factory.ForceStringEncoding = _forceStringEncoding;
            factory.Radix = _radix;

            return factory;
        }
    }
}
