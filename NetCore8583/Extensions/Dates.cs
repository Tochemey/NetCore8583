using System;

namespace NetCore8583.Extensions
{
    /// <summary>Date/time utilities for ISO 8583 (e.g. Unix time in milliseconds).</summary>
    public static class Dates
    {
        private static readonly DateTime Jan1St1970 = new(1970,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        /// <summary>Returns the current UTC time as milliseconds since 1970-01-01 00:00:00 UTC.</summary>
        /// <returns>Unix timestamp in milliseconds.</returns>
        public static long CurrentTimeMillis()
        {
            return (long) (DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }
    }
}