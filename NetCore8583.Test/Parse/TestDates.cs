using System;
using System.IO;
using NetCore8583.Parse;
using NetCore8583.Util;
using TimeZoneConverter;
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

            Assert.Equal(soon.Month, v.Month);
            Assert.Equal(soon.Day, v.Day);
            Assert.Equal(soon.Hour, v.Hour);
            Assert.Equal(soon.Minute, v.Minute);
            Assert.Equal(soon.Second, v.Second);

            var stream = new MemoryStream();
            comp.Write(stream, true, false);
            var bin = new Date10ParseInfo().ParseBinary(0, stream.ToArray().ToInt8(), 0, null);
            var dt = (DateTime) comp.Value;
            var bindt = (DateTime) bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate12FutureTolerance()
        {
            var today = DateTime.UtcNow;
            var soon = today.AddMilliseconds(50000); 
            var buf = IsoType.DATE12.Format(soon).GetSignedBytes();
            var comp = new Date12ParseInfo().Parse(0, buf, 0, null);
            var v = (DateTime) comp.Value;

            Assert.Equal(soon.Month, v.Month);
            Assert.Equal(soon.Day, v.Day);
            Assert.Equal(soon.Hour, v.Hour);
            Assert.Equal(soon.Minute, v.Minute);
            Assert.Equal(soon.Second, v.Second);

            var stream = new MemoryStream();
            comp.Write(stream, true, false);
            var bin = new Date12ParseInfo().ParseBinary(0, stream.ToArray().ToInt8(), 0, null);
            var dt = (DateTime) comp.Value;
            var bindt = (DateTime) bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate14FutureTolerance()
        {
            var today = DateTime.UtcNow;
            var soon = today.AddMilliseconds(50000); 
            var buf = IsoType.DATE14.Format(soon).GetSignedBytes();
            var comp = new Date14ParseInfo().Parse(0, buf, 0, null);
            var v = (DateTime) comp.Value;

            Assert.Equal(soon.Month, v.Month);
            Assert.Equal(soon.Day, v.Day);
            Assert.Equal(soon.Hour, v.Hour);
            Assert.Equal(soon.Minute, v.Minute);
            Assert.Equal(soon.Second, v.Second);

            var stream = new MemoryStream();
            comp.Write(stream, true, false);
            var bin = new Date14ParseInfo().ParseBinary(0, stream.ToArray().ToInt8(), 0, null);
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
            var bin = new Date4ParseInfo().ParseBinary(0, stream.ToArray().ToInt8(), 0, null);
            var dt = (DateTime) comp.Value;
            var bindt = (DateTime) bin.Value;
            Assert.Equal(dt.ToBinary(), bindt.ToBinary());
        }

        [Fact]
        public void TestDate6()
        {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeMilliseconds(1514700000000L);
            var tz = TZConvert.GetTimeZoneInfo("Central Standard Time");
            date = TimeZoneInfo.ConvertTime(date,
                tz);
            
            IsoValue f = new IsoValue(IsoType.DATE6, date.DateTime);
            Assert.Equal("171231", f.ToString());
            Assert.Equal("171231", IsoType.DATE6.Format(date));
            var parsed = new Date6ParseInfo().Parse(1, "171231".GetSignedBytes(), 0, null);
            Assert.Equal("171231", parsed.ToString());
            Assert.Equal(f.Value,parsed.Value);

            var buf = new sbyte[3];
            buf[0] = 0x17;
            buf[1] = 0x12;
            buf[2] = 0x31;
            var dt = new Date6ParseInfo();
            parsed = dt.ParseBinary(2, buf, 0, null);
            Assert.Equal("171231", parsed.ToString());
            Assert.Equal(f.Value,parsed.Value);
        }
    }
}
