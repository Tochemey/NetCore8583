using System.Collections.Generic;

namespace NetCore8583.Extensions
{
    /// <summary>Extension methods for <see cref="Dictionary{TKey,TValue}"/> used by the ISO 8583 library.</summary>
    public static class DictionaryExtensions
    {
        /// <summary>Adds all key-value pairs from <paramref name="val"/> into <paramref name="dic"/>.</summary>
        /// <param name="dic">The dictionary to add to.</param>
        /// <param name="val">The dictionary to copy from.</param>
        /// <typeparam name="TK">Key type.</typeparam>
        /// <typeparam name="TV">Value type.</typeparam>
        public static void AddAll<TK, TV>(this Dictionary<TK, TV> dic,
            Dictionary<TK, TV> val)
        {
            foreach (var (key, value) in val) dic.Add(key, value);
        }
    }
}