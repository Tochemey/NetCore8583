using System;

namespace NetCore8583.Util
{
    public class ParseException : FormatException
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}