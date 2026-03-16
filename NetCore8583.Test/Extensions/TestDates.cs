using System;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestDates
    {
        [Fact]
        public void CurrentTimeMillisIsPositive()
        {
            Assert.True(Dates.CurrentTimeMillis() > 0);
        }

        [Fact]
        public void CurrentTimeMillisApproximatesSystemTime()
        {
            var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var result = Dates.CurrentTimeMillis();
            var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Assert.InRange(result, before, after + 100);
        }

        [Fact]
        public void ConsecutiveCallsAreNonDecreasing()
        {
            var first = Dates.CurrentTimeMillis();
            var second = Dates.CurrentTimeMillis();
            Assert.True(second >= first);
        }
    }
}
