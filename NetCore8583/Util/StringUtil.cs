using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetCore8583.Util
{
    /// <summary>
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        ///     Check emptiness.True when it is empty and false on the contrary
        /// </summary>
        /// <param name="string0">String to check</param>
        /// <returns>bool. True when it is empty and false on the contrary</returns>
        public static bool IsEmpty(this string string0)
        {
            return string.IsNullOrEmpty(string0);
        }
        
        public static byte[] GetBytes(this string check)
        {
            return Encoding.UTF8.GetBytes(check);
        }

        public static sbyte[] GetSignedBytes(this string check,
            Encoding encoding = null)
        {
            var bytes = encoding == null ? Encoding.Default.GetBytes(check) : encoding.GetBytes(check);
            return Array.ConvertAll(bytes,
                b => unchecked((sbyte) b));
        }
    }
}