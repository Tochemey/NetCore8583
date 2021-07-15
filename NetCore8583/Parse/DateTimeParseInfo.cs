using System;

namespace NetCore8583.Parse
{
    public abstract class DateTimeParseInfo : FieldParseInfo
    {
        private const long FutureTolerance = 900000L;

        protected DateTimeParseInfo(IsoType isoType, int length) : base(isoType, length)
        {
        }

        public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

        protected static DateTimeOffset AdjustWithFutureTolerance(DateTimeOffset calendar)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var then = calendar.ToUnixTimeMilliseconds();
            if (then > now && then - now > FutureTolerance) return calendar.AddYears(-1);
            return calendar;
        }
    }
}