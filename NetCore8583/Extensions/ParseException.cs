using System;

namespace NetCore8583.Extensions
{
    /// <summary>Exception thrown when ISO 8583 message or field parsing fails (e.g. invalid length, insufficient data).</summary>
    public class ParseException : FormatException
    {
        /// <summary>Creates a parse exception with the given message.</summary>
        /// <param name="message">Description of the parse error.</param>
        public ParseException(string message) : base(message)
        {
        }
    }
}