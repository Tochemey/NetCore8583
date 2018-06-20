using System;

namespace NetCore8583.Parse
{
    public abstract class DateTimeParseInfo : FieldParseInfo
    {
        protected static readonly long FUTURE_TOLERANCE = 900000L;

        public DateTimeParseInfo(IsoType isoType,
            int length) : base(isoType,
            length)
        {
        }

        public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

        public static DateTimeOffset AdjustWithFutureTolerance(DateTimeOffset calendar)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var then = calendar.ToUnixTimeMilliseconds();
            if (then > now && then - now > FUTURE_TOLERANCE) return calendar.AddYears(-1);
            return calendar;
        }
    }
}