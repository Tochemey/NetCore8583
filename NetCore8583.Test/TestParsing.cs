using System;
using System.Text;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test
{
    public class TestParsing
    {
        public TestParsing()
        {
            mf = new MessageFactory<IsoMessage>();
            mf.Encoding = Encoding.UTF8;
            mf.SetCustomField(48,
                new CustomField48());
            mf.SetConfigPath(@"/Resources/config.xml");
        }

        private readonly MessageFactory<IsoMessage> mf;

        [Fact]
        public void TestBinaryNumberParsing()
        {
            var npi = new NumericParseInfo(6);
            var val = npi.ParseBinary(0, new byte[] {0x12, 0x34, 0x56}.ToInt8(), 0, null);
            Assert.Equal(123456, Convert.ToInt32(val.Value));
        }

        [Fact]
        public void TestEmpty()
        {
            ;
            Assert.Throws<ParseException>(() => mf.ParseMessage(new sbyte[0],
                0));
        }

        [Fact]
        public void TestIncompleteFixedField()
        {
            Assert.Throws<ParseException>(() => mf.ParseMessage(
                "0210B23A80012EA08018000000001400000465000".GetSignedBytes(),
                0));
        }

        [Fact]
        public void TestIncompleteFixedFieldBin()
        {
            mf.UseBinaryMessages = true;
            Assert.Throws<ParseException>(() => mf.ParseMessage(new byte[]
                {
                    2,
                    0x10,
                    0xB2,
                    0x3A,
                    0x80,
                    1,
                    0x2E,
                    0xA0,
                    0x80,
                    0x18,
                    0,
                    0,
                    0,
                    0,
                    0x14,
                    0,
                    0,
                    4,
                    0x65,
                    0
                }.ToInt8(),
                0));
        }

        [Fact]
        public void TestIncompleteVarFieldData()
        {
            Assert.Throws<ParseException>(() => mf.ParseMessage(
                "0210B23A80012EA0801800000000140000046500000000000030000428130547468771125946042804280811051234"
                    .GetSignedBytes(),
                0));
        }

        [Fact]
        public void TestIncompleteVarFieldDataBin()
        {
            mf.UseBinaryMessages = true;
            Assert.Throws<ParseException>(() => mf.ParseMessage(new byte[]
                {
                    2,
                    0x10,
                    0xB2,
                    0x3A,
                    0x80,
                    1,
                    0x2E,
                    0xA0,
                    0x80,
                    0x18,
                    0,
                    0,
                    0,
                    0,
                    0x14,
                    0,
                    0,
                    4,
                    0x65,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0x30,
                    0,
                    0x04,
                    0x28,
                    0x13,
                    0x05,
                    0x47,
                    0x46,
                    0x87,
                    0x71,
                    0x12,
                    0x59,
                    0x46,
                    0x04,
                    0x28,
                    0x04,
                    0x28,
                    0x08,
                    0x11,
                    0x05,
                    0x12,
                    0x34
                }.ToInt8(),
                0));
        }

        [Fact]
        public void TestIncompleteVarFieldHeader()
        {
            Assert.Throws<ParseException>(() => mf.ParseMessage(
                "0210B23A80012EA08018000000001400000465000000000000300004281305474687711259460428042808115"
                    .GetSignedBytes(),
                0));
        }

        [Fact]
        public void TestIncompleteVarFieldHeaderBin()
        {
            mf.UseBinaryMessages = true;
            Assert.Throws<ParseException>(() => mf.ParseMessage(new byte[]
                {
                    2,
                    0x10,
                    0xB2,
                    0x3A,
                    0x80,
                    1,
                    0x2E,
                    0xA0,
                    0x80,
                    0x18,
                    0,
                    0,
                    0,
                    0,
                    0x14,
                    0,
                    0,
                    4,
                    0x65,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0x30,
                    0,
                    0x04,
                    0x28,
                    0x13,
                    0x05,
                    0x47,
                    0x46,
                    0x87,
                    0x71,
                    0x12,
                    0x59,
                    0x46,
                    0x04,
                    0x28,
                    0x04,
                    0x28,
                    0x08,
                    0x11
                }.ToInt8(),
                0));
        }

        [Fact]
        public void TestNoFields()
        {
            Assert.Throws<ParseException>(() => mf.ParseMessage("0210B23A80012EA080180000000014000004".GetSignedBytes(),
                0));
        }

        [Fact]
        public void TestNoFieldsBin()
        {
            mf.UseBinaryMessages = true;
            Assert.Throws<ParseException>(() => mf.ParseMessage(new byte[]
                {
                    2,
                    0x10,
                    0xB2,
                    0x3A,
                    0x80,
                    1,
                    0x2E,
                    0xA0,
                    0x80,
                    0x18,
                    0,
                    0,
                    0,
                    0,
                    0x14,
                    0,
                    0,
                    4
                }.ToInt8(),
                0));
        }

        [Fact]
        public void TestShort()
        {
            Assert.Throws<ParseException>(() => mf.ParseMessage(new sbyte[20],
                8));
        }

        [Fact]
        public void TestShortBin()
        {
            Assert.Throws<ParseException>(() => mf.ParseMessage(new sbyte[10],
                1));
        }

        [Fact]
        public void TestShortSecondaryBitmap()
        {
            Assert.Throws<ParseException>(() => mf.ParseMessage("02008000000000000000".GetSignedBytes(),
                0));
        }

        [Fact]
        public void TestShortSecondaryBitmapBin()
        {
            mf.UseBinaryMessages = true;
            Assert.Throws<ParseException>(() => mf.ParseMessage(new byte[]
                {
                    2,
                    0,
                    128,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0
                }.ToInt8(),
                0));
        }
    }
}