using System;

namespace NetCore8583.Util
{
    public static class DateUtil
    {
        private static readonly DateTime Jan1St1970 = new(1970,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long) (DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }
    }
}