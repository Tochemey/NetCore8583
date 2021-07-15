using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCore8583.Parse;
using NetCore8583.Util;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace NetCore8583
{
    /// <summary>
    ///     This class is used to create messages, either from scratch or from an existing String or byte
    ///     buffer. It can be configured to put default values on newly created messages, and also to know
    ///     what to expect when reading messages from a Stream.
    ///     The factory can be configured to know what values to set for newly created messages, both from
    ///     a template (useful for fields that must be set with the same value for EVERY message created)
    ///     and individually (for trace [field 11] and message date [field 7]).
    ///     It can also be configured to know what fields to expect in incoming messages (all possible values
    ///     must be stated, indicating the date type for each). This way the messages can be parsed from
    ///     a byte buffer.
    /// </summary>
    public class MessageFactory<T> where T : IsoMessage
    {
        /// <summary>
        /// </summary>
        private readonly Dictionary<int, sbyte[]> _binIsoHeaders = new Dictionary<int, sbyte[]>();

        /// <summary>
        ///     A map for the custom field encoder/decoders, keyed by field number.
        /// </summary>
        private readonly Dictionary<int, ICustomField> _customFields = new Dictionary<int, ICustomField>();

        /// <summary>
        ///     The ISO header to be included in each message type
        /// </summary>
        private readonly Dictionary<int, string> _isoHeaders = new Dictionary<int, string>();

        private readonly Logger _logger = new LoggerConfiguration().MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console().CreateLogger();

        /// <summary>
        ///     This map stores the message template for each message type.
        /// </summary>
        private readonly Dictionary<int, T> _typeTemplates = new Dictionary<int, T>();

        private Encoding _encoding = Encoding.Default;
        private bool _forceStringEncoding;
        private int _radix = 10;

        /// <summary>
        ///     Stores the information needed to parse messages sorted by type
        /// </summary>
        protected Dictionary<int, Dictionary<int, FieldParseInfo>> ParseMap =
            new Dictionary<int, Dictionary<int, FieldParseInfo>>();

        /// <summary>
        ///     Stores the field numbers to be parsed, in order of appearance.
        /// </summary>
        protected Dictionary<int, List<int>> ParseOrder = new Dictionary<int, List<int>>();

        public ITraceNumberGenerator TraceGenerator { get; set; }

        /// <summary>
        ///     Indicates if the current date should be set on new messages (field 7).
        /// </summary>
        public bool AssignDate { get; set; }

        /// <summary>
        ///     Indicates if the factory should create binary messages and also parse binary messages.
        /// </summary>
        public bool UseBinaryMessages { get; set; }

        /// <summary>
        /// </summary>
        public int Etx { get; set; } = -1;

        public bool IgnoreLast { get; set; }

        public bool EnforceSecondBitmap { get; set; }
        public bool UseBinaryBitmap { get; set; }

        public bool ForceStringEncoding
        {
            get => _forceStringEncoding;
            set
            {
                _forceStringEncoding = value;
                foreach (var parser in ParseMap.Values.SelectMany(mapValue => mapValue.Values))
                    parser.ForceStringDecoding = value;
            }
        }

        public int Radix
        {
            get => _radix;
            set
            {
                _radix = value;
                foreach (var parser in ParseMap.Values.SelectMany(mapValue => mapValue.Values))
                    parser.Radix = value;
            }
        }


        public Encoding Encoding
        {
            get => _encoding;
            set
            {
                _encoding = value;
                if (_encoding == null) throw new ArgumentException("Cannot set null encoding.");

                if (ParseMap.Count > 0)
                    foreach (var fpi in ParseMap.Values.SelectMany(mapValue => mapValue.Values))
                        fpi.Encoding = value;

                if (_typeTemplates.Count == 0) return;

                foreach (var tmpl in _typeTemplates.Values)
                {
                    tmpl.Encoding = value;
                    for (var i = 2; i < 129; i++)
                    {
                        var v = tmpl.GetField(i);
                        if (v != null) v.Encoding = value;
                    }
                }
            }
        }

        public void SetCustomField(int index,
            ICustomField value)
        {
            _customFields.Add(index,
                value);
        }

        public ICustomField GetCustomField(int index)
        {
            return _customFields.ContainsKey(index) ? _customFields[index] : null;
        }

        protected T CreateIsoMessageWithBinaryHeader(sbyte[] binHeader)
        {
            return (T) new IsoMessage(binHeader);
        }

        protected T CreateIsoMessage(string isoHeader)
        {
            return (T) new IsoMessage(isoHeader);
        }

        /// <summary>
        ///     Creates a new message of the specified type, with optional trace and date values as well
        ///     as any other values specified in a message template. If the factory is set to use binary
        ///     messages, then the returned message will be written using binary coding.
        /// </summary>
        /// <param name="type">The message type, for example 0x200, 0x400, etc.</param>
        /// <returns></returns>
        public T NewMessage(int type)
        {
            var keyPresent = _binIsoHeaders.ContainsKey(type);
            sbyte[] val = null;
            var valStr = string.Empty;

            if (keyPresent)
                val = _binIsoHeaders[type];
            else if (_isoHeaders.ContainsKey(type))
                valStr = _isoHeaders[type];

            var m = keyPresent ? CreateIsoMessageWithBinaryHeader(val) : CreateIsoMessage(valStr);
            m.Type = type;
            m.Etx = Etx;
            m.Binary = UseBinaryMessages;
            m.EnforceSecondBitmap = EnforceSecondBitmap;
            m.BinBitmap = UseBinaryBitmap;
            m.Encoding = Encoding;
            m.ForceStringEncoding = ForceStringEncoding;

            //Copy the values from the template
            IsoMessage templ = _typeTemplates.ContainsKey(type) ? _typeTemplates[type] : null;
            if (templ != null)
                for (var i = 2; i <= 128; i++)
                    if (templ.HasField(i))
                        m.SetField(i,
                            (IsoValue) templ.GetField(i).Clone());
            if (TraceGenerator != null)
                m.SetValue(11,
                    TraceGenerator.NextTrace(),
                    IsoType.NUMERIC,
                    6);
            if (AssignDate)
                m.SetValue(7,
                    DateTime.Now,
                    IsoType.DATE10,
                    10);
            return m;
        }

        /// <summary>
        ///     Creates a message to respond to a request. Increments the message type by 16,
        ///     sets all fields from the template if there is one, and copies all values from the request,
        ///     overwriting fields from the template if they overlap.
        /// </summary>
        /// <param name="request">An ISO8583 message with a request type (ending in 00).</param>
        /// <param name="copyAllFields">
        ///     If true, copies all fields from the request to the response, overwriting any values already
        ///     set from the template; otherwise it only overwrites values for existing fields from the template. If the template
        ///     for a response does not exist, then all fields from
        /// </param>
        /// <returns></returns>
        public T CreateResponse(T request, bool copyAllFields = true)
        {
            var resp = CreateIsoMessage(_isoHeaders[request.Type + 16]);
            resp.Encoding = request.Encoding;
            resp.Binary = request.Binary;
            resp.BinBitmap = request.BinBitmap;
            resp.Type = request.Type + 16;
            resp.Etx = request.Etx;
            resp.EnforceSecondBitmap = request.EnforceSecondBitmap;
            IsoMessage templ = _typeTemplates[resp.Type];
            if (templ == null)
            {
                for (var i = 2; i < 128; i++)
                    if (request.HasField(i))
                        resp.SetField(i,
                            request.GetField(i).Clone() as IsoValue);
            }
            else if (copyAllFields)
            {
                for (var i = 2; i < 128; i++)
                    if (request.HasField(i))
                        resp.SetField(i,
                            request.GetField(i).Clone() as IsoValue);
                    else if (templ.HasField(i))
                        resp.SetField(i,
                            templ.GetField(i).Clone() as IsoValue);
            }
            else
            {
                for (var i = 2; i < 128; i++)
                    if (templ.HasField(i))
                    {
                        var src = request.HasField(i) ? request : templ;
                        resp.SetField(i, src.GetField(i).Clone() as IsoValue);
                    }
            }

            return resp;
        }

        /// <summary>
        ///     Sets the timezone for the specified FieldParseInfo, if it's needed for parsing dates.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="field"></param>
        /// <param name="tz"></param>
        public void SetTimezoneForParseGuide(int messageType,
            int field,
            TimeZoneInfo tz)
        {
            var guide = ParseMap[messageType];
            var fpi = guide?[field];
            if (fpi is DateTimeParseInfo dateTimeParseInfo)
            {
                dateTimeParseInfo.TimeZoneInfo = tz;
                return;
            }

            _logger.Warning("Field {@Field} for message type {@MessageType} is not for dates, cannot set timezone",
                field,
                messageType);
        }

        /// <summary>
        ///     Parses a byte buffer containing an ISO8583 message. The buffer must
        ///     not include the length header. If it includes the ISO message header,
        ///     then its length must be specified so the message type can be found.
        /// </summary>
        /// <param name="buf">
        ///     The byte buffer containing the message, starting
        ///     at the ISO header or the message type.
        /// </param>
        /// <param name="isoHeaderLength">
        ///     Specifies the position at which the message
        ///     type is located, which is also the length of the ISO header.
        /// </param>
        /// <param name="binaryIsoHeader"></param>
        /// <returns>The parsed message.</returns>
        public T ParseMessage(sbyte[] buf,
            int isoHeaderLength,
            bool binaryIsoHeader = false)
        {
            var minlength = isoHeaderLength + (UseBinaryMessages ? 2 : 4) +
                            (UseBinaryBitmap || UseBinaryMessages ? 8 : 16);
            if (buf.Length < minlength)
                throw new ParseException("Insufficient buffer length, needs to be at least " + minlength);
            T m;
            if (binaryIsoHeader && isoHeaderLength > 0)
            {
                var bih = new sbyte[isoHeaderLength];
                Array.Copy(buf,
                    0,
                    bih,
                    0,
                    isoHeaderLength);
                m = CreateIsoMessageWithBinaryHeader(bih);
            }
            else
            {
                var string0 = buf.ToString(0,
                    isoHeaderLength,
                    _encoding);
                m = CreateIsoMessage(isoHeaderLength > 0 ? string0 : null);
            }

            m.Encoding = _encoding;
            int type;
            if (UseBinaryMessages)
            {
                type = ((buf[isoHeaderLength] & 0xff) << 8) | (buf[isoHeaderLength + 1] & 0xff);
            }
            else if (ForceStringEncoding)
            {
                var string0 = buf.ToString(isoHeaderLength,
                    4,
                    _encoding);
                type = Convert.ToInt32(string0,
                    16);
            }
            else
            {
                type = ((buf[isoHeaderLength] - 48) << 12)
                       | ((buf[isoHeaderLength + 1] - 48) << 8)
                       | ((buf[isoHeaderLength + 2] - 48) << 4)
                       | (buf[isoHeaderLength + 3] - 48);
            }

            m.Type = type;
            //Parse the bitmap (primary first)
            var bs = new BitArray(64);
            var pos = 0;
            if (UseBinaryMessages || UseBinaryBitmap)
            {
                var bitmapStart = isoHeaderLength + (UseBinaryMessages ? 2 : 4);
                for (var i = bitmapStart; i < 8 + bitmapStart; i++)
                {
                    var bit = 128;
                    for (var b = 0; b < 8; b++)
                    {
                        bs.Set(pos++,
                            (buf[i] & bit) != 0);
                        bit >>= 1;
                    }
                }

                //Check for secondary bitmap and parse if necessary
                if (bs.Get(0))
                {
                    bs.Length = 128;
                    if (buf.Length < minlength + 8)
                        throw new ParseException($"Insufficient length for secondary bitmap : {minlength}");

                    for (var i = 8 + bitmapStart; i < 16 + bitmapStart; i++)
                    {
                        var bit = 128;
                        for (var b = 0; b < 8; b++)
                        {
                            bs.Set(pos++,
                                (buf[i] & bit) != 0);
                            bit >>= 1;
                        }
                    }

                    pos = minlength + 8;
                }
                else
                {
                    pos = minlength;
                }
            }
            else
            {
                //ASCII parsing
                try
                {
                    sbyte[] bitmapBuffer;
                    if (ForceStringEncoding)
                    {
                        var bb = buf.ToString(isoHeaderLength + 4,
                            16,
                            _encoding).GetSignedBytes();

                        bitmapBuffer = new sbyte[36 + isoHeaderLength];
                        Array.Copy(bb,
                            0,
                            bitmapBuffer,
                            4 + isoHeaderLength,
                            16);
                    }
                    else
                    {
                        bitmapBuffer = buf;
                    }

                    for (var i = isoHeaderLength + 4; i < isoHeaderLength + 20; i++)
                        if (bitmapBuffer[i] >= '0' && bitmapBuffer[i] <= '9')
                        {
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 48) & 8) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 48) & 4) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 48) & 2) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 48) & 1) > 0);
                        }
                        else if (bitmapBuffer[i] >= 'A' && bitmapBuffer[i] <= 'F')
                        {
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 55) & 8) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 55) & 4) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 55) & 2) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 55) & 1) > 0);
                        }
                        else if (bitmapBuffer[i] >= 'a' && bitmapBuffer[i] <= 'f')
                        {
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 87) & 8) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 87) & 4) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 87) & 2) > 0);
                            bs.Set(pos++,
                                ((bitmapBuffer[i] - 87) & 1) > 0);
                        }

                    //Check for secondary bitmap and parse it if necessary
                    if (bs.Get(0))
                    {
                        bs.Length = 128;
                        if (buf.Length < minlength + 16)
                            throw new ParseException($"Insufficient length for secondary bitmap :{minlength}");
                        if (ForceStringEncoding)
                        {
                            var bb = buf.ToString(isoHeaderLength + 20,
                                16,
                                _encoding).GetSignedBytes();
                            Array.Copy(bb,
                                0,
                                bitmapBuffer,
                                20 + isoHeaderLength,
                                16);
                        }

                        for (var i = isoHeaderLength + 20; i < isoHeaderLength + 36; i++)
                            if (bitmapBuffer[i] >= '0' && bitmapBuffer[i] <= '9')
                            {
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 48) & 8) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 48) & 4) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 48) & 2) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 48) & 1) > 0);
                            }
                            else if (bitmapBuffer[i] >= 'A' && bitmapBuffer[i] <= 'F')
                            {
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 55) & 8) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 55) & 4) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 55) & 2) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 55) & 1) > 0);
                            }
                            else if (bitmapBuffer[i] >= 'a' && bitmapBuffer[i] <= 'f')
                            {
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 87) & 8) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 87) & 4) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 87) & 2) > 0);
                                bs.Set(pos++,
                                    ((bitmapBuffer[i] - 87) & 1) > 0);
                            }

                        pos = 16 + minlength;
                    }
                    else
                    {
                        pos = minlength;
                    }
                }
                catch (FormatException e)
                {
                    var exception = new ParseException($"Invalid ISO8583 bitmap: Cause {e}");
                    throw exception;
                }
            }

            //Parse each field
            var parseGuide = ParseMap[type];
            var index = ParseOrder[type];
            if (index == null)
            {
                _logger.Error(
                    $"ISO8583 MessageFactory has no parsing guide for message type {type:X} [{buf.ToString(0, buf.Length, _encoding)}]");
                throw new Exception(
                    $"ISO8583 MessageFactory has no parsing guide for message type {type:X} [{buf.ToString(0, buf.Length, _encoding)}]");
            }

            //First we check if the message contains fields not specified in the parsing template
            var abandon = false;
            for (var i = 1; i < bs.Length; i++)
                if (bs.Get(i) && !index.Contains(i + 1))
                {
                    _logger.Warning("ISO8583 MessageFactory cannot parse field {Field}: unspecified in parsing guide",
                        i + 1);
                    abandon = true;
                }

            if (abandon) throw new ParseException("ISO8583 MessageFactory cannot parse fields");

            //Now we parse each field
            if (UseBinaryMessages)
                foreach (var i in index)
                {
                    var fpi = parseGuide[i];
                    if (!bs.Get(i - 1)) continue;
                    if (IgnoreLast && pos >= buf.Length && i == index[^1])
                    {
                        _logger.Warning("Field {@Index} is not really in the message even though it's in the bitmap",
                            i);

                        bs.Set(i - 1,
                            false);
                    }
                    else
                    {
                        var decoder = fpi.Decoder ?? GetCustomField(i);

                        var val = fpi.ParseBinary(i,
                            buf,
                            pos,
                            decoder);

                        m.SetField(i,
                            val);

                        if (val == null) continue;
                        if (val.Type == IsoType.NUMERIC || val.Type == IsoType.DATE10 || val.Type == IsoType.DATE4 ||
                            val.Type == IsoType.DATE12 || val.Type == IsoType.DATE14 || val.Type == IsoType.DATE6 ||
                            val.Type == IsoType.DATE_EXP ||
                            val.Type == IsoType.AMOUNT || val.Type == IsoType.TIME)
                            pos += val.Length / 2 + val.Length % 2;
                        else pos += val.Length;
                        switch (val.Type)
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
                    }
                }
            else
                foreach (var i in index)
                {
                    var fpi = parseGuide[i];
                    if (bs.Get(i - 1))
                        if (IgnoreLast && pos >= buf.Length && i == index[^1])
                        {
                            _logger.Warning(
                                "Field {@FieldId} is not really in the message even though it's in the bitmap",
                                i);
                            bs.Set(i - 1,
                                false);
                        }
                        else
                        {
                            var decoder = fpi.Decoder ?? GetCustomField(i);
                            var val = fpi.Parse(i,
                                buf,
                                pos,
                                decoder);
                            m.SetField(i,
                                val);
                            //To get the correct next position, we need to get the number of bytes, not chars
                            pos += val.ToString().GetSignedBytes(fpi.Encoding).Length;
                            switch (val.Type)
                            {
                                case IsoType.LLVAR:
                                case IsoType.LLBIN:
                                    pos += 2;
                                    break;
                                case IsoType.LLLVAR:
                                case IsoType.LLLBIN:
                                    pos += 3;
                                    break;
                                case IsoType.LLLLVAR:
                                case IsoType.LLLLBIN:
                                    pos += 4;
                                    break;
                            }
                        }
                }

            m.Binary = UseBinaryMessages;
            m.BinBitmap = UseBinaryBitmap;
            return m;
        }

        /// <summary>
        ///     Sets the ISO header to be used in each message type.
        /// </summary>
        /// <param name="val">A map where the keys are the message types and the values are the ISO headers.</param>
        public void SetIsoHeaders(Dictionary<int, string> val)
        {
            _isoHeaders.Clear();
            foreach (var (key, value) in val) _isoHeaders.Add(key, value);
        }

        /// <summary>
        ///     Sets the ISO header for a specific message type.
        /// </summary>
        /// <param name="type">The message type, for example 0x200</param>
        /// <param name="val">The ISO header, or NULL to remove any headers for this message type.</param>
        public void SetIsoHeader(int type,
            string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                _isoHeaders.Remove(type);
            }
            else
            {
                if (_isoHeaders.ContainsKey(type)) _isoHeaders[type] = val;
                else
                    _isoHeaders.Add(type,
                        val);

                _binIsoHeaders.Remove(type);
            }
        }

        /// <summary>
        ///     Returns the ISO header used for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetIsoHeader(int type)
        {
            return _isoHeaders[type];
        }

        /// <summary>
        ///     Sets the ISO header for a specific message type, in binary format.
        /// </summary>
        /// <param name="type">The message type, for example 0x200.</param>
        /// <param name="val">The ISO header, or NULL to remove any headers for this message type.</param>
        public void SetBinaryIsoHeader(int type,
            byte[] val)
        {
            if (val == null)
            {
                _binIsoHeaders.Remove(type);
            }
            else
            {
                if (_binIsoHeaders.ContainsKey(type)) _binIsoHeaders[type] = val.ToInt8();
                else
                    _binIsoHeaders.Add(type,
                        val.ToInt8());

                _isoHeaders.Remove(type);
            }
        }

        /// <summary>
        ///     Returns the binary ISO header used for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public sbyte[] GetBinaryIsoHeader(int type)
        {
            return _binIsoHeaders[type];
        }

        /// <summary>
        ///     Adds a message template to the factory. If there was a template for the same
        ///     message type as the new one, it is overwritten.
        /// </summary>
        /// <param name="templ"></param>
        public void AddMessageTemplate(T templ)
        {
            if (templ == null) return;
            if (_typeTemplates.ContainsKey(templ.Type)) _typeTemplates[templ.Type] = templ;
            else
                _typeTemplates.Add(templ.Type,
                    templ);
        }

        /// <summary>
        ///     Removes the message template for the specified type.
        /// </summary>
        /// <param name="type"></param>
        public void RemoveMessageTemplate(int type)
        {
            _typeTemplates.Remove(type);
        }

        /// <summary>
        ///     Returns the template for the specified message type. This allows templates to be modified
        ///     programmatically.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public T GetMessageTemplate(int type)
        {
            return _typeTemplates[type];
        }

        public void SetParseMap(int type,
            Dictionary<int, FieldParseInfo> map)
        {
            if (ParseMap.ContainsKey(type)) ParseMap[type] = map;
            else
                ParseMap.Add(type,
                    map);

            var index = new List<int>();
            index.AddRange(map.Keys);
            index.Sort();
            _logger.Warning($"ISO8583 MessageFactory adding parse map for type {type:X} with fields {index}");

            if (ParseOrder.ContainsKey(type)) ParseOrder[type] = index;
            else
                ParseOrder.Add(type,
                    index);
        }

        /// <summary>
        ///     Tells the receiver to read the configuration at the specified path. This just calls
        ///     ConfigParser.configureFromClasspathConfig() with itself and the specified path at arguments,
        ///     but is really convenient in case the MessageFactory is being configured from within
        /// </summary>
        /// <param name="path"></param>
        public void SetConfigPath(string path)
        {
            ConfigParser.ConfigureFromClasspathConfig(this,
                path);
            //Now re-set some properties that need to be propagated down to the recently assigned objects
            Encoding = _encoding;
            ForceStringEncoding = _forceStringEncoding;
        }
    }
}