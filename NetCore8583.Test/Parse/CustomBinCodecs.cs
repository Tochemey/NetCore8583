using System;
using System.Collections.Generic;
using System.Numerics;
using NetCore8583.Codecs;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    public class CustomBinCodecs
    {
        private readonly BigInteger b29 = BigInteger.Parse("12345678901234567890123456789");

        private readonly sbyte[] longData2 =
        {
            0x12,
            0x34,
            0x56,
            0x78,
            unchecked((sbyte) 0x90),
            00,
            00,
            00,
            00,
            00
        };

        private readonly sbyte[] bigintData1 =
        {
            1,
            0x23,
            0x45,
            0x67,
            unchecked((sbyte) 0x89),
            1,
            0x23,
            0x45,
            0x67,
            unchecked((sbyte) 0x89),
            1,
            0x23,
            0x45,
            0x67,
            unchecked((sbyte) 0x89),
            00,
            00,
            00,
            00,
            00
        };

        private void TestFieldType(IsoType type,
            FieldParseInfo fieldParser,
            int offset1,
            int offset2)
        {
            var bigintCodec = new BigIntBcdCodec();
            var longCodec = new LongBcdCodec();
            var mfact = new MessageFactory<IsoMessage>();
            var tmpl = new IsoMessage
            {
                Binary = true,
                Type = 0x200
            };
            tmpl.SetValue(2,
                1234567890L,
                longCodec,
                type,
                0);
            tmpl.SetValue(3,
                b29,
                bigintCodec,
                type,
                0);
            mfact.AddMessageTemplate(tmpl);
            mfact.SetCustomField(2,
                longCodec);
            mfact.SetCustomField(3,
                bigintCodec);
            var parser = new Dictionary<int, FieldParseInfo>
            {
                {2, fieldParser},
                {3, fieldParser}
            };
            mfact.SetParseMap(0x200,
                parser);
            mfact.UseBinaryMessages = true;

            //Test encoding
            tmpl = mfact.NewMessage(0x200);
            var buf = tmpl.WriteData();
            var message = HexCodec.HexEncode(buf,
                0,
                buf.Length);
            Console.WriteLine("MESSAGE: " + message);
            for (var i = 0; i < 5; i++)
            {
                var b = longData2[i];
                Assert.Equal(b,
                    buf[i + offset1]);
            }

            for (var i = 0; i < 15; i++)
                Assert.Equal(bigintData1[i],
                    buf[i + offset2]);

            //Test parsing
            tmpl = mfact.ParseMessage(buf,
                0);
            Assert.Equal(1234567890L,
                tmpl.GetObjectValue(2));
            Assert.Equal(b29,
                tmpl.GetObjectValue(3));
        }

        [Fact]
        public void TestBigIntCodec()
        {
            var b30 = BigInteger.Parse("123456789012345678901234567890");
            var bigintCodec = new BigIntBcdCodec();
            sbyte[] data2 =
            {
                0x12,
                0x34,
                0x56,
                0x78,
                unchecked((sbyte) 0x90),
                0x12,
                0x34,
                0x56,
                0x78,
                unchecked((sbyte) 0x90),
                0x12,
                0x34,
                0x56,
                0x78,
                unchecked((sbyte) 0x90),
                00,
                00,
                00,
                00,
                00
            };
            Assert.Equal(b29,
                bigintCodec.DecodeBinaryField(bigintData1,
                    0,
                    15));
            Assert.Equal(b30,
                bigintCodec.DecodeBinaryField(data2,
                    0,
                    15));
            var cod1 = bigintCodec.EncodeBinaryField(b29);
            var cod2 = bigintCodec.EncodeBinaryField(b30);
            for (var i = 0; i < 15; i++)
            {
                Assert.Equal(bigintData1[i],
                    cod1[i]);
                Assert.Equal(data2[i],
                    cod2[i]);
            }
        }

        [Fact]
        public void TestLLBIN()
        {
            TestFieldType(IsoType.LLBIN,
                new LlbinParseInfo(),
                11,
                17);
        }

        [Fact]
        public void TestLLLBIN()
        {
            TestFieldType(IsoType.LLLBIN,
                new LllbinParseInfo(),
                12,
                19);
        }


        [Fact]
        public void TestLongCodec()
        {
            var longCodec = new LongBcdCodec();
            sbyte[] data1 =
            {
                1,
                0x23,
                0x45,
                0x67,
                unchecked((sbyte) 0x89),
                00,
                00,
                00,
                00,
                00
            };
            Assert.Equal(123456789L,
                (long) longCodec.DecodeBinaryField(data1,
                    0,
                    5));
            Assert.Equal(1234567890L,
                (long) longCodec.DecodeBinaryField(longData2,
                    0,
                    5));
            var cod1 = longCodec.EncodeBinaryField(123456789L);
            var cod2 = longCodec.EncodeBinaryField(1234567890L);
            for (var i = 0; i < 5; i++)
            {
                Assert.Equal(data1[i],
                    cod1[i]);
                Assert.Equal(longData2[i],
                    cod2[i]);
            }
        }
    }
}