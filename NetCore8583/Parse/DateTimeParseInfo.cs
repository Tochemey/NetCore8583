using System;

namespace NetCore8583.Parse
{
    /// <summary>Base parse info for date/time ISO field types (DATE4, DATE6, DATE10, DATE12, DATE14, DATE_EXP, TIME).</summary>
    public abstract class DateTimeParseInfo : FieldParseInfo
    {
        private const long FutureTolerance = 900000L;

        /// <summary>Initializes parse info for a date/time field type.</summary>
        /// <param name="isoType">The ISO date/time type.</param>
        /// <param name="length">The formatted length in characters or bytes.</param>
        protected DateTimeParseInfo(IsoType isoType, int length) : base(isoType, length)
        {
        }

        /// <summary>Time zone used when interpreting and formatting date/time values. Default is <see cref="TimeZoneInfo.Local"/>.</summary>
        public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

        /// <summary>If the parsed date is in the future beyond a tolerance window, adjusts it back one year (e.g. for YY rollover).</summary>
        /// <param name="calendar">The parsed date/time.</param>
        /// <returns>The possibly adjusted date/time.</returns>
        protected static DateTimeOffset AdjustWithFutureTolerance(DateTimeOffset calendar)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var then = calendar.ToUnixTimeMilliseconds();
            if (then > now && then - now > FutureTolerance) return calendar.AddYears(-1);
            return calendar;
        }
    }
}