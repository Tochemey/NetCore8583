using System;
using System.Text;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestStringx
    {
        [Fact]
        public void TestGetSignedBytes_DefaultEncoding()
        {
            var text = "ISO8583";
            var result = text.GetSignedBytes();
            
            Assert.NotNull(result);
            Assert.Equal(7, result.Length);
        }

        [Fact]
        public void TestGetSignedBytes_UTF8()
        {
            var text = "Hello World";
            var result = text.GetSignedBytes(Encoding.UTF8);
            
            var expected = Encoding.UTF8.GetBytes(text).AsSignedBytes().ToArray();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestGetSignedBytes_EmptyString()
        {
            var text = "";
            var result = text.GetSignedBytes(Encoding.UTF8);
            
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void TestGetSignedBytes_SingleCharacter()
        {
            var text = "A";
            var result = text.GetSignedBytes(Encoding.ASCII);
            
            Assert.Single(result);
            Assert.Equal(65, result[0]);
        }

        [Fact]
        public void TestGetSignedBytes_SpecialCharacters()
        {
            var text = "Test©®™€";
            var result = text.GetSignedBytes(Encoding.UTF8);
            
            var roundTrip = result.ToString(Encoding.UTF8);
            Assert.Equal(text, roundTrip);
        }

        [Fact]
        public void TestGetSignedBytes_DifferentEncodings()
        {
            var text = "ISO8583";
            
            var encodings = new[]
            {
                Encoding.UTF8,
                Encoding.ASCII,
                Encoding.Unicode,
                Encoding.UTF32
            };
            
            foreach (var encoding in encodings)
            {
                var result = text.GetSignedBytes(encoding);
                var roundTrip = result.ToString(encoding);
                
                Assert.Equal(text, roundTrip);
            }
        }

        [Fact]
        public void TestGetSignedBytes_LargeString()
        {
            var text = new string('X', 10000);
            var result = text.GetSignedBytes(Encoding.UTF8);
            
            Assert.Equal(10000, result.Length);
            Assert.All(result, b => Assert.Equal((sbyte)'X', b));
        }

        [Fact]
        public void TestGetSignedBytes_NullEncoding_UsesDefault()
        {
            var text = "Test";
            var result = text.GetSignedBytes(null);
            
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TestTryGetSignedBytes_SuccessfulEncode()
        {
            var text = "ISO8583";
            Span<sbyte> buffer = stackalloc sbyte[128];
            
            var success = text.TryGetSignedBytes(buffer, out int written, Encoding.UTF8);
            
            Assert.True(success);
            Assert.Equal(7, written);
            
            ReadOnlySpan<sbyte> resultSpan = buffer.Slice(0, written);
            var result = resultSpan.ToString(Encoding.UTF8);
            Assert.Equal(text, result);
        }

        [Fact]
        public void TestTryGetSignedBytes_BufferTooSmall()
        {
            var text = "Very long ISO8583 message that won't fit";
            Span<sbyte> buffer = stackalloc sbyte[5];
            
            var success = text.TryGetSignedBytes(buffer, out int written, Encoding.UTF8);
            
            Assert.False(success);
            Assert.Equal(0, written);
        }

        [Fact]
        public void TestTryGetSignedBytes_ExactSize()
        {
            var text = "Test";
            Span<sbyte> buffer = stackalloc sbyte[4];
            
            var success = text.TryGetSignedBytes(buffer, out int written, Encoding.UTF8);
            
            Assert.True(success);
            Assert.Equal(4, written);
        }

        [Fact]
        public void TestTryGetSignedBytes_EmptyString()
        {
            var text = "";
            Span<sbyte> buffer = stackalloc sbyte[10];
            
            var success = text.TryGetSignedBytes(buffer, out int written, Encoding.UTF8);
            
            Assert.True(success);
            Assert.Equal(0, written);
        }

        [Fact]
        public void TestGetSignedBytes_WithSpanBuffer()
        {
            var text = "ISO8583";
            Span<sbyte> buffer = stackalloc sbyte[128];
            
            var written = text.GetSignedBytes(buffer, Encoding.UTF8);
            
            Assert.Equal(7, written);
            
            ReadOnlySpan<sbyte> resultSpan2 = buffer.Slice(0, written);
            var result = resultSpan2.ToString(Encoding.UTF8);
            Assert.Equal(text, result);
        }

        [Fact]
        public void TestGetSignedBytes_WithSpanBuffer_LargeText()
        {
            var text = new string('A', 1000);
            Span<sbyte> buffer = stackalloc sbyte[1024];
            
            var written = text.GetSignedBytes(buffer, Encoding.UTF8);
            
            Assert.Equal(1000, written);
        }

        [Fact]
        public void TestGetSignedBytesCount()
        {
            var text = "ISO8583";
            var count = text.GetSignedBytesCount(Encoding.UTF8);
            
            Assert.Equal(7, count);
            
            var actual = text.GetSignedBytes(Encoding.UTF8);
            Assert.Equal(count, actual.Length);
        }

        [Fact]
        public void TestGetSignedBytesCount_EmptyString()
        {
            var text = "";
            var count = text.GetSignedBytesCount(Encoding.UTF8);
            
            Assert.Equal(0, count);
        }

        [Fact]
        public void TestGetSignedBytesCount_MultiByteCharacters()
        {
            var text = "€";
            var count = text.GetSignedBytesCount(Encoding.UTF8);
            
            Assert.Equal(3, count);
            
            var actual = text.GetSignedBytes(Encoding.UTF8);
            Assert.Equal(count, actual.Length);
        }

        [Fact]
        public void TestGetSignedBytesPooled()
        {
            var text = "ISO8583 Message";
            var result = text.GetSignedBytesPooled(Encoding.UTF8);
            
            var direct = text.GetSignedBytes(Encoding.UTF8);
            Assert.Equal(direct, result);
        }

        [Fact]
        public void TestGetSignedBytesPooled_LargeString()
        {
            var text = new string('X', 10000);
            var result = text.GetSignedBytesPooled(Encoding.UTF8);
            
            Assert.Equal(10000, result.Length);
            Assert.All(result, b => Assert.Equal((sbyte)'X', b));
        }

        [Fact]
        public void TestGetSignedBytesPooled_EmptyString()
        {
            var text = "";
            var result = text.GetSignedBytesPooled(Encoding.UTF8);
            
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void TestStackAllocPattern_ZeroHeapAllocation()
        {
            var text = "Short";
            Span<sbyte> buffer = stackalloc sbyte[128];
            
            var written = text.GetSignedBytes(buffer, Encoding.UTF8);
            var slice = buffer.Slice(0, written);
            
            Assert.Equal(5, slice.Length);
            ReadOnlySpan<sbyte> sliceRo = slice;
            Assert.Equal(text, sliceRo.ToString(Encoding.UTF8));
        }

        [Fact]
        public void TestRoundTrip_AllMethods()
        {
            var original = "ISO8583 Test Message ©®™";
            
            var method1 = original.GetSignedBytes(Encoding.UTF8);
            var roundTrip1 = method1.ToString(Encoding.UTF8);
            Assert.Equal(original, roundTrip1);
            
            Span<sbyte> buffer = stackalloc sbyte[256];
            var written = original.GetSignedBytes(buffer, Encoding.UTF8);
            ReadOnlySpan<sbyte> spanRo = buffer.Slice(0, written);
            var roundTrip2 = spanRo.ToString(Encoding.UTF8);
            Assert.Equal(original, roundTrip2);
            
            var method3 = original.GetSignedBytesPooled(Encoding.UTF8);
            var roundTrip3 = method3.ToString(Encoding.UTF8);
            Assert.Equal(original, roundTrip3);
        }

        [Fact]
        public void TestAllMethods_ProduceSameResult()
        {
            var text = "Compare All Methods";
            
            var result1 = text.GetSignedBytes(Encoding.UTF8);
            
            Span<sbyte> buffer = stackalloc sbyte[256];
            var written = text.GetSignedBytes(buffer, Encoding.UTF8);
            var result2 = buffer.Slice(0, written).ToArray();
            
            var result3 = text.GetSignedBytesPooled(Encoding.UTF8);
            
            Assert.Equal(result1, result2);
            Assert.Equal(result1, result3);
        }

        [Fact]
        public void TestTryPattern_VariousSizes()
        {
            var texts = new[]
            {
                "",
                "A",
                "Short",
                "Medium length text",
                "Very long text that spans multiple cache lines and tests buffer sizing"
            };
            
            Span<sbyte> buffer = stackalloc sbyte[1024];
            
            foreach (var text in texts)
            {
                var expected = text.GetSignedBytes(Encoding.UTF8);
                
                var success = text.TryGetSignedBytes(buffer, out int written, Encoding.UTF8);
                
                Assert.True(success);
                Assert.Equal(expected.Length, written);
                
                var actual = buffer.Slice(0, written).ToArray();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void TestPreCalculateSize_AllocateExact()
        {
            var text = "Calculate exact size";
            var size = text.GetSignedBytesCount(Encoding.UTF8);
            
            Span<sbyte> buffer = stackalloc sbyte[size];
            var written = text.GetSignedBytes(buffer, Encoding.UTF8);
            
            Assert.Equal(size, written);
            ReadOnlySpan<sbyte> bufferRo = buffer;
            Assert.Equal(text, bufferRo.ToString(Encoding.UTF8));
        }

        [Fact]
        public void TestIsEmpty_Extension()
        {
            Assert.True(((string)null).IsEmpty());
            Assert.True("".IsEmpty());
            Assert.False("Test".IsEmpty());
        }

        [Fact]
        public void TestGetBytes_Extension()
        {
            var text = "Test";
            var result = text.GetBytes();
            
            Assert.Equal(Encoding.UTF8.GetBytes(text), result);
        }

        [Fact]
        public void TestNullEncoding_DefaultBehavior()
        {
            var text = "Test with null encoding";
            
            var result1 = text.GetSignedBytes(null);
            var result2 = text.GetSignedBytes(Encoding.Default);
            
            Assert.Equal(result2, result1);
        }

        [Fact]
        public void TestAllEncodingMethods_ConsistentResults()
        {
            var text = "Consistency Test";
            var encoding = Encoding.UTF8;
            
            var standard = text.GetSignedBytes(encoding);
            
            Span<sbyte> buffer = stackalloc sbyte[256];
            text.TryGetSignedBytes(buffer, out int written, encoding);
            var tryMethod = buffer.Slice(0, written).ToArray();
            
            var pooled = text.GetSignedBytesPooled(encoding);
            
            Assert.Equal(standard, tryMethod);
            Assert.Equal(standard, pooled);
        }
    }
}
