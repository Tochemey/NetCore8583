using System;
using System.IO;
using NetCore8583.Parse;
using NetCore8583.Util;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class TestDates
    {
        [Fact]
        public void TestDate10FutureTolerance()
        {
            var today = DateTime.UtcNow;
            var soon = today.AddMilliseconds(50000);
            var buf = IsoType.DATE10.Format(soon).GetSignedBytes();
            var comp = new Date10ParseInfo().Parse(0, buf, 0, null);
            var v = (DateTime) comp.Value;
            Assert.True(v.CompareTo(DateTime.Now) > 0);
            var stream = new MemoryStream();
            comp.Write(stream, true, false);
            var bin = new Date10ParseInfo().ParseBinary(0, stream.ToArray().ToSignedBytes(), 0, null);
            var dt = (DateTime) comp.Value;
            var bindt = (DateTime) bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate12FutureTolerance()
        {
            var soon = DateTime.UtcNow.AddMilliseconds(50000);
            var buf = IsoType.DATE12.Format(soon).GetSignedBytes();
            var comp = new Date12ParseInfo().Parse(0, buf, 0, null);
            var v = (DateTime) comp.Value;
            Assert.True(v.CompareTo(DateTime.UtcNow) > 0);
            var stream = new MemoryStream();
            comp.Write(stream, true, false);
            var bin = new Date12ParseInfo().ParseBinary(0, stream.ToArray().ToSignedBytes(), 0, null);
            var dt = (DateTime) comp.Value;
            var bindt = (DateTime) bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate14FutureTolerance()
        {
            var soon = DateTime.UtcNow.AddMilliseconds(50000);
            var buf = IsoType.DATE14.Format(soon).GetSignedBytes();
            var comp = new Date14ParseInfo().Parse(0, buf, 0, null);
            var v = (DateTime) comp.Value;
            Assert.True(v.CompareTo(DateTime.UtcNow) > 0);
            var stream = new MemoryStream();
            comp.Write(stream, true, false);
            var bin = new Date14ParseInfo().ParseBinary(0, stream.ToArray().ToSignedBytes(), 0, null);
            var dt = (DateTime) comp.Value;
            var bindt = (DateTime) bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate4FutureTolerance()
        {
            var today = DateTime.UtcNow;
            var soon = today.AddMilliseconds(50000);
            today = today.AddHours(0)
                .AddMinutes(0)
                .AddSeconds(0)
                .AddMilliseconds(0);
            var buf = IsoType.DATE4.Format(soon).GetSignedBytes();
            var comp = new Date4ParseInfo().Parse(0,
                buf,
                0,
                null);
            Assert.Equal(comp.Value,
                today.Date);
            var stream = new MemoryStream();
            comp.Write(stream, true, false);
            var bin = new Date4ParseInfo().ParseBinary(0, stream.ToArray().ToSignedBytes(), 0, null);
            var dt = (DateTime) comp.Value;
            var bindt = (DateTime) bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }
    }
}