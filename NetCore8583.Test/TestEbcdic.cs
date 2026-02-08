using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test
{
    public class TestEbcdic
    {
        private readonly IsoValue llvar = new IsoValue(IsoType.LLVAR,
            "Testing, testing, 123");

        [Fact]
        public void TestAmount()
        {
            var parser = new AmountParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };

            var v = parser.Parse(4,
                new[]
                {
                    unchecked((sbyte) 240),
                    unchecked((sbyte) 240),
                    unchecked((sbyte) 240),
                    unchecked((sbyte) 241),
                    unchecked((sbyte) 242),
                    unchecked((sbyte) 243),
                    unchecked((sbyte) 244),
                    unchecked((sbyte) 245),
                    unchecked((sbyte) 246),
                    unchecked((sbyte) 247),
                    unchecked((sbyte) 248),
                    unchecked((sbyte) 249)
                },
                0,
                null);
            Assert.Equal(decimal.Parse("1234567.89"),
                v.Value);
        }

        [Fact]
        public void TestAscii()
        {
            llvar.Encoding = Encoding.UTF8;
            var bout = new MemoryStream();
            llvar.Write(bout,
                false,
                false);
            var buf = bout.ToArray().ToInt8();
            Assert.Equal(50,
                buf[0]);
            Assert.Equal(49,
                buf[1]);
            var parser = new LlvarParseInfo
            {
                Encoding = Encoding.UTF8
            };

            var field = parser.Parse(1,
                buf,
                0,
                null);
            Assert.Equal(llvar.Value,
                field.Value);
        }

        [Fact]
        public void TestDate10()
        {
            var parser = new Date10ParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };
            var v = parser.Parse(1,
                new[]
                {
                    (byte) 240,
                    (byte) 241,
                    (byte) 242,
                    (byte) 245,
                    (byte) 242,
                    (byte) 243,
                    (byte) 245,
                    (byte) 249,
                    (byte) 245,
                    (byte) 249
                }.ToInt8(),
                0,
                null);
            var val = (DateTime) v.Value;
            Assert.Equal("01",
                val.ToString("MM"));
            Assert.Equal("25",
                val.ToString("dd"));
            Assert.Equal("23:59:59",
                val.ToString("HH:mm:ss"));
        }

        [Fact]
        public void TestDate4()
        {
            var parser = new Date4ParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };
            var v = parser.Parse(1,
                new[]
                {
                    (byte) 240,
                    (byte) 241,
                    (byte) 242,
                    (byte) 245
                }.ToInt8(),
                0,
                null);
            var val = (DateTime) v.Value;
            Assert.Equal("01",
                val.ToString("MM"));
            Assert.Equal("25",
                val.ToString("dd"));
        }

        [Fact]
        public void TestDateExp()
        {
            var parser = new DateExpParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };

            var v = parser.Parse(1,
                new[]
                {
                    (byte) 241,
                    (byte) 247,
                    (byte) 241,
                    (byte) 242
                }.ToInt8(),
                0,
                null);
            var val = (DateTime) v.Value;
            Assert.Equal("12",
                val.ToString("MM"));
            Assert.Equal("2017",
                val.ToString("yyyy"));
        }

        [Fact]
        public void TestEbcdic0()
        {
            llvar.Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047);
            var bout = new MemoryStream();
            llvar.Write(bout,
                false,
                true);
            var buf = bout.ToArray().ToInt8();
            Assert.Equal(unchecked((sbyte) 242),
                buf[0]);
            Assert.Equal(unchecked((sbyte) 241),
                buf[1]);
            var parser = new LlvarParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };
            var field = parser.Parse(1,
                buf,
                0,
                null);
            Assert.Equal(llvar.Value,
                field.Value);
        }

        [Fact]
        public void TestMessage()
        {
            var trama = HexCodec.HexDecode(
                "f1f8f1f42030010002000000f0f0f0f0f0f0f0f1f5f9f5f5f1f3f0f6f1f2f1f1f2f9f0f8f8f3f1f8f0f0");
            var mfact = new MessageFactory<IsoMessage>
            {
                UseBinaryBitmap = true
            };
            var pinfo = new Dictionary<int, FieldParseInfo>
            {
                {3, new AlphaParseInfo(6)},
                {11, new AlphaParseInfo(6)},
                {12, new AlphaParseInfo(12)},
                {24, new AlphaParseInfo(3)},
                {39, new AlphaParseInfo(3)}
            };
            mfact.SetParseMap(0x1814,
                pinfo);
            mfact.Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047);
            mfact.ForceStringEncoding = true;
            var iso = mfact.ParseMessage(trama,
                0);
            Assert.NotNull(iso);
            Assert.Equal("000000",
                iso.GetObjectValue(3));
            Assert.Equal("015955",
                iso.GetObjectValue(11));
            Assert.Equal("130612112908",
                iso.GetObjectValue(12));
            Assert.Equal("831",
                iso.GetObjectValue(24));
            Assert.Equal("800",
                iso.GetObjectValue(39));
        }

        [Fact]
        public void TestMessageType()
        {
            var msg = new IsoMessage
            {
                Type = 0x1100,
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                BinBitmap = true
            };
            var enc = msg.WriteData();
            Assert.Equal(12,
                enc.Length);
            Assert.Equal(unchecked((sbyte) 241),
                enc[0]);
            Assert.Equal(unchecked((sbyte) 241),
                enc[1]);
            Assert.Equal(unchecked((sbyte) 240),
                enc[2]);
            Assert.Equal(unchecked((sbyte) 240),
                enc[3]);
            var mf = new MessageFactory<IsoMessage>();
            var pmap = new Dictionary<int, FieldParseInfo>();
            mf.ForceStringEncoding = true;
            mf.UseBinaryBitmap = true;
            mf.Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047);
            mf.SetParseMap(0x1100,
                pmap);
            var m2 = mf.ParseMessage(enc,
                0);
            Assert.Equal(msg.Type,
                m2.Type);

            //Now with text bitmap
            msg.BinBitmap = false;
            msg.ForceStringEncoding = true;
            var enc2 = msg.WriteData();
            Assert.Equal(20,
                enc2.Length);
            mf.UseBinaryBitmap = false;
            m2 = mf.ParseMessage(enc2,
                0);
            Assert.Equal(msg.Type,
                m2.Type);
        }

        [Fact]
        public void TestParsers()
        {
            var stringA = CodePagesEncodingProvider.Instance.GetEncoding(1047).GetBytes("A");
            var lllvar = new LllvarParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };
            var field = lllvar.Parse(1,
                new[]
                {
                    (byte) 240,
                    (byte) 240,
                    (byte) 241,
                    (byte) 193
                }.ToInt8(),
                0,
                null);
            var string0 = CodePagesEncodingProvider.Instance.GetEncoding(1047).GetString(stringA);
            Assert.Equal(string0,
                field.Value);

            var lllbin = new LllbinParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };
            field = lllbin.Parse(1,
                new byte[]
                {
                    240,
                    240,
                    242,
                    67,
                    49
                }.ToInt8(),
                0,
                null);
            Assert.Equal(stringA.ToInt8(),
                (sbyte[]) field.Value);

            var llbin = new LlbinParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };
            field = llbin.Parse(1,
                new byte[]
                {
                    240,
                    242,
                    67,
                    49
                }.ToInt8(),
                0,
                null);
            Assert.Equal(stringA.ToInt8(),
                (sbyte[]) field.Value);
        }

        [Fact]
        public void TestTime()
        {
            var parser = new TimeParseInfo
            {
                Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1047),
                ForceStringDecoding = true
            };
            var v = parser.Parse(1,
                new[]
                {
                    (byte) 242,
                    (byte) 241,
                    (byte) 243,
                    (byte) 244,
                    (byte) 245,
                    (byte) 246
                }.ToInt8(),
                0,
                null);
            var val = (DateTime) v.Value;
            Assert.Equal("21:34:56",
                val.ToString("HH:mm:ss"));
        }
    }
}