using System;
using System.Linq;
using System.Text;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestBytes
    {
        [Fact]
        public void TestToInt8_EquivalentToAsSignedBytes()
        {
            var unsigned = new byte[] { 0x00, 0x7F, 0x80, 0xFF, 0x01, 0xFE };
        
            var oldResult = unsigned.ToInt8();
            var newResult = unsigned.AsSignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToInt8_EmptyArray()
        {
            var unsigned = new byte[] { };
            
            var oldResult = unsigned.ToInt8();
            var newResult = unsigned.AsSignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToInt8_SingleByte()
        {
            var unsigned = new byte[] { 0x42 };
            
            var oldResult = unsigned.ToInt8();
            var newResult = unsigned.AsSignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToInt8_AllZeros()
        {
            var unsigned = new byte[100];
            
            var oldResult = unsigned.ToInt8();
            var newResult = unsigned.AsSignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToInt8_AllOnes()
        {
            var unsigned = Enumerable.Repeat((byte)0xFF, 100).ToArray();
            
            var oldResult = unsigned.ToInt8();
            var newResult = unsigned.AsSignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToInt8_LargeArray()
        {
            var unsigned = new byte[1000];
            for (int i = 0; i < unsigned.Length; i++)
            {
                unsigned[i] = (byte)(i % 256);
            }
            
            var oldResult = unsigned.ToInt8();
            var newResult = unsigned.AsSignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToUint8_EquivalentToAsUnsignedBytes()
        {
            var signed = new sbyte[] { 0, 127, -128, -1, 1, -2 };
            
            var oldResult = signed.ToUint8();
            var newResult = signed.AsUnsignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToUint8_EmptyArray()
        {
            var signed = new sbyte[] { };
            
            var oldResult = signed.ToUint8();
            var newResult = signed.AsUnsignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToUint8_SingleByte()
        {
            var signed = new sbyte[] { -42 };
            
            var oldResult = signed.ToUint8();
            var newResult = signed.AsUnsignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToUint8_AllPositive()
        {
            var signed = Enumerable.Range(0, 100).Select(i => (sbyte)i).ToArray();
            
            var oldResult = signed.ToUint8();
            var newResult = signed.AsUnsignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToUint8_AllNegative()
        {
            var signed = Enumerable.Range(-100, 100).Select(i => (sbyte)i).ToArray();
            
            var oldResult = signed.ToUint8();
            var newResult = signed.AsUnsignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestToUint8_LargeArray()
        {
            var signed = new sbyte[1000];
            for (int i = 0; i < signed.Length; i++)
            {
                signed[i] = (sbyte)((i % 256) - 128);
            }
            
            var oldResult = signed.ToUint8();
            var newResult = signed.AsUnsignedBytes().ToArray();
            
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void TestRoundTrip_ByteToSByteToByteArray()
        {
            var original = new byte[] { 0, 1, 127, 128, 255 };
            
            var signed = original.ToInt8();
            var roundTrip = signed.ToUint8();
            
            Assert.Equal(original, roundTrip);
        }

        [Fact]
        public void TestRoundTrip_ByteToSByteToByteSpan()
        {
            var original = new byte[] { 0, 1, 127, 128, 255 };
            
            var signed = original.AsSignedBytes().ToArray();
            var roundTrip = signed.AsUnsignedBytes().ToArray();
            
            Assert.Equal(original, roundTrip);
        }

        [Fact]
        public void TestToString_ArrayMethod_EquivalentResults()
        {
            var data = "Hello ISO8583!";
            var bytes = Encoding.UTF8.GetBytes(data);
            var signed = bytes.AsSignedBytes().ToArray();
            
            var oldResult = signed.ToString(Encoding.UTF8);
            var newResult = ((ReadOnlySpan<sbyte>)signed).ToString(Encoding.UTF8);
            
            Assert.Equal(oldResult, newResult);
            Assert.Equal(data, oldResult);
        }

        [Fact]
        public void TestToString_WithPositionAndLength_EquivalentResults()
        {
            var data = "Hello ISO8583!";
            var bytes = Encoding.UTF8.GetBytes(data);
            var signed = bytes.AsSignedBytes().ToArray();
            
            var oldResult = signed.ToString(6, 7, Encoding.UTF8);
            var newResult = ((ReadOnlySpan<sbyte>)signed).ToString(6, 7, Encoding.UTF8);
            
            Assert.Equal(oldResult, newResult);
            Assert.Equal("ISO8583", oldResult);
        }

        [Fact]
        public void TestToString_EmptyString()
        {
            var signed = new sbyte[0];
            
            var oldResult = signed.ToString(Encoding.UTF8);
            var newResult = ((ReadOnlySpan<sbyte>)signed).ToString(Encoding.UTF8);
            
            Assert.Equal(oldResult, newResult);
            Assert.Equal(string.Empty, oldResult);
        }

        [Fact]
        public void TestToString_DifferentEncodings()
        {
            var data = "ISO8583 Message";
            
            var encodings = new[] 
            { 
                Encoding.UTF8, 
                Encoding.ASCII, 
                Encoding.Unicode,
                Encoding.UTF32
            };
            
            foreach (var encoding in encodings)
            {
                var bytes = encoding.GetBytes(data);
                var signed = bytes.AsSignedBytes().ToArray();
                
                var oldResult = signed.ToString(encoding);
                var newResult = ((ReadOnlySpan<sbyte>)signed).ToString(encoding);
                
                Assert.Equal(oldResult, newResult);
                Assert.Equal(data, oldResult);
            }
        }

        [Fact]
        public void TestToString_WithSpecialCharacters()
        {
            var data = "Test\n\r\t©®™€";
            var bytes = Encoding.UTF8.GetBytes(data);
            var signed = bytes.AsSignedBytes().ToArray();
            
            var oldResult = signed.ToString(Encoding.UTF8);
            var newResult = ((ReadOnlySpan<sbyte>)signed).ToString(Encoding.UTF8);
            
            Assert.Equal(oldResult, newResult);
            Assert.Equal(data, oldResult);
        }

        [Fact]
        public void TestToString_Slice()
        {
            var data = "0123456789";
            var bytes = Encoding.UTF8.GetBytes(data);
            var signed = bytes.AsSignedBytes().ToArray();
            
            for (int start = 0; start < data.Length; start++)
            {
                for (int len = 0; len <= data.Length - start; len++)
                {
                    var expected = data.Substring(start, len);
                    var oldResult = signed.ToString(start, len, Encoding.UTF8);
                    var newResult = ((ReadOnlySpan<sbyte>)signed).ToString(start, len, Encoding.UTF8);
                    
                    Assert.Equal(oldResult, newResult);
                    Assert.Equal(expected, oldResult);
                }
            }
        }

        [Fact]
        public void TestAsSignedBytes_ModificationAffectsBoth()
        {
            var unsigned = new byte[] { 1, 2, 3, 4, 5 };
            var signed = unsigned.AsSignedBytes();
            
            signed[2] = 99;
            
            Assert.Equal(99, (sbyte)unsigned[2]);
        }

        [Fact]
        public void TestAsUnsignedBytes_ModificationAffectsBoth()
        {
            var signed = new sbyte[] { 1, 2, 3, 4, 5 };
            var unsigned = signed.AsUnsignedBytes();
            
            unsigned[2] = 99;
            
            Assert.Equal(99, signed[2]);
        }

        [Fact]
        public void TestReadOnlySpan_Conversions()
        {
            var unsigned = new byte[] { 0xFF, 0x01, 0x7F, 0x80 };
            ReadOnlySpan<byte> unsignedSpan = unsigned;
            
            var signed = unsignedSpan.AsSignedBytes();
            
            Assert.Equal(-1, signed[0]);
            Assert.Equal(1, signed[1]);
            Assert.Equal(127, signed[2]);
            Assert.Equal(-128, signed[3]);
        }

        [Fact]
        public void TestSpan_SliceConversions()
        {
            var unsigned = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var slice = unsigned.AsSpan(3, 4);
            var signed = slice.AsSignedBytes();
            
            Assert.Equal(4, signed.Length);
            Assert.Equal(3, signed[0]);
            Assert.Equal(4, signed[1]);
            Assert.Equal(5, signed[2]);
            Assert.Equal(6, signed[3]);
        }

        [Fact]
        public void TestBitPatternPreservation()
        {
            var testCases = new byte[] { 0x00, 0x7F, 0x80, 0xFF };
            
            foreach (var b in testCases)
            {
                var unsigned = new byte[] { b };
                
                var oldSigned = unsigned.ToInt8()[0];
                var newSigned = unsigned.AsSignedBytes()[0];
                
                Assert.Equal(oldSigned, newSigned);
                
                var signedArray = new sbyte[] { newSigned };
                var oldUnsigned = signedArray.ToUint8()[0];
                var newUnsigned = signedArray.AsUnsignedBytes()[0];
                
                Assert.Equal(oldUnsigned, newUnsigned);
                Assert.Equal(b, newUnsigned);
            }
        }

        [Fact]
        public void TestSpanMethod_SpanToSpanConversion()
        {
            Span<sbyte> signed = stackalloc sbyte[] { 1, 2, 3, 4, 5 };
            var unsigned = signed.AsUnsignedBytes();
            
            Assert.Equal(5, unsigned.Length);
            Assert.Equal(1, unsigned[0]);
            Assert.Equal(5, unsigned[4]);
        }

        [Fact]
        public void TestPerformanceCharacteristics_NoAllocation()
        {
            var data = new byte[1000];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }
            
            var signed = data.AsSignedBytes();
            
            Assert.Equal(1000, signed.Length);
            Assert.Equal(0, signed[0]);
            Assert.Equal(unchecked((sbyte)255), signed[255]);
        }
    }
}
