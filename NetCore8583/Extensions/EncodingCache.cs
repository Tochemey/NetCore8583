using System.Text;

namespace NetCore8583.Extensions
{
    /// <summary>
    ///     Provides cached encoding instances to avoid repeated allocations.
    ///     Encoding objects are thread-safe and reusable.
    /// </summary>
    public static class EncodingCache
    {
        /// <summary>
        ///     Cached UTF-8 encoding instance (same as Encoding.UTF8 but explicit).
        /// </summary>
        public static readonly Encoding Utf8 = Encoding.UTF8;

        /// <summary>
        ///     Cached Default encoding instance (platform default).
        /// </summary>
        public static readonly Encoding Default = Encoding.Default;

        /// <summary>
        ///     Cached ASCII encoding instance.
        /// </summary>
        public static readonly Encoding Ascii = Encoding.ASCII;

        /// <summary>
        ///     Cached UTF-16 (Unicode) encoding instance.
        /// </summary>
        public static readonly Encoding Unicode = Encoding.Unicode;

        /// <summary>
        ///     Cached UTF-32 encoding instance.
        /// </summary>
        public static readonly Encoding Utf32 = Encoding.UTF32;
    }
}
