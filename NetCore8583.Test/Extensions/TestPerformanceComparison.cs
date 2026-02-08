using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NetCore8583.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace NetCore8583.Test.Extensions
{
    public class TestPerformanceComparison
    {
        private readonly ITestOutputHelper _output;

        public TestPerformanceComparison(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CompareByteToSByteConversion_SmallArray()
        {
            var data = new byte[100];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }

            const int iterations = 10000;
            
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = data.ToInt8();
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = data.AsSignedBytes().ToArray();
            }
            sw2.Stop();

            _output.WriteLine($"ToInt8 (old): {sw1.ElapsedMilliseconds}ms");
            _output.WriteLine($"AsSignedBytes (new): {sw2.ElapsedMilliseconds}ms");
            _output.WriteLine($"Speedup: {(double)sw1.ElapsedTicks / sw2.ElapsedTicks:F2}x");
            
            var oldResult = data.ToInt8();
            var newResult = data.AsSignedBytes().ToArray();
            Assert.Equal(oldResult, newResult);
        }

        [Fact]
        public void CompareByteToSByteConversion_LargeArray()
        {
            var data = new byte[10000];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i % 256);
            }

            const int iterations = 1000;
            
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = data.ToInt8();
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                var result = data.AsSignedBytes().ToArray();
            }
            sw2.Stop();

            _output.WriteLine($"ToInt8 (old) - 10KB: {sw1.ElapsedMilliseconds}ms");
            _output.WriteLine($"AsSignedBytes (new) - 10KB: {sw2.ElapsedMilliseconds}ms");
            _output.WriteLine($"Speedup: {(double)sw1.ElapsedTicks / sw2.ElapsedTicks:F2}x");
        }

        [Fact]
        public void CompareSByteToByteConversion_SmallArray()
        {
            var data = new sbyte[100];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (sbyte)(i - 50);
            }

            const int iterations = 10000;
            
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = data.ToUint8();
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = data.AsUnsignedBytes().ToArray();
            }
            sw2.Stop();

            _output.WriteLine($"ToUint8 (old): {sw1.ElapsedMilliseconds}ms");
            _output.WriteLine($"AsUnsignedBytes (new): {sw2.ElapsedMilliseconds}ms");
            _output.WriteLine($"Speedup: {(double)sw1.ElapsedTicks / sw2.ElapsedTicks:F2}x");
        }

        [Fact]
        public void CompareStringEncoding_ShortStrings()
        {
            var text = "ISO8583";
            const int iterations = 100000;
            
            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = text.GetSignedBytes(Encoding.UTF8);
            }
            sw2.Stop();

            _output.WriteLine($"GetSignedBytes (optimized): {sw2.ElapsedMilliseconds}ms");
            
            var result1 = text.GetSignedBytes(Encoding.UTF8);
            Assert.Equal(7, result1.Length);
        }

        [Fact]
        public void CompareStringEncoding_LongStrings()
        {
            var text = new string('X', 1000);
            const int iterations = 10000;
            
            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = text.GetSignedBytes(Encoding.UTF8);
            }
            sw2.Stop();

            _output.WriteLine($"GetSignedBytes (optimized) - 1KB: {sw2.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void CompareStringEncoding_ZeroAllocation()
        {
            var text = "ISO8583";
            const int iterations = 100000;
            
            Span<sbyte> buffer = stackalloc sbyte[128];
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var written = text.GetSignedBytes(buffer, Encoding.UTF8);
            }
            sw.Stop();

            _output.WriteLine($"GetSignedBytes with buffer (zero-alloc): {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void CompareBitmapOperations_ZeroCopyVsCopy()
        {
            var unsigned = new byte[16];
            for (int i = 0; i < unsigned.Length; i++)
            {
                unsigned[i] = (byte)(i * 16);
            }

            const int iterations = 100000;
            
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var signed = unsigned.ToInt8();
                var back = signed.ToUint8();
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var signed = unsigned.AsSignedBytes();
                var back = signed.AsUnsignedBytes();
            }
            sw2.Stop();

            _output.WriteLine($"Old (with copy): {sw1.ElapsedMilliseconds}ms");
            _output.WriteLine($"New (span view): {sw2.ElapsedMilliseconds}ms");
            _output.WriteLine($"Speedup: {(double)sw1.ElapsedTicks / sw2.ElapsedTicks:F2}x");
        }

        [Fact]
        public void CompareRoundTripConversion()
        {
            var original = new byte[500];
            for (int i = 0; i < original.Length; i++)
            {
                original[i] = (byte)i;
            }

            const int iterations = 10000;
            
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var signed = original.ToInt8();
                var back = signed.ToUint8();
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var signed = original.AsSignedBytes().ToArray();
                var back = signed.AsUnsignedBytes().ToArray();
            }
            sw2.Stop();

            _output.WriteLine($"Round-trip old: {sw1.ElapsedMilliseconds}ms");
            _output.WriteLine($"Round-trip new: {sw2.ElapsedMilliseconds}ms");
            _output.WriteLine($"Improvement: {(1 - (double)sw2.ElapsedTicks / sw1.ElapsedTicks) * 100:F1}%");
        }

        [Fact]
        public void CompareToStringMethods()
        {
            var text = "ISO 8583 Financial Transaction Message";
            var bytes = Encoding.UTF8.GetBytes(text);
            var signed = bytes.AsSignedBytes().ToArray();

            const int iterations = 100000;

            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = signed.ToString(Encoding.UTF8);
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = ((ReadOnlySpan<sbyte>)signed).ToString(Encoding.UTF8);
            }
            sw2.Stop();

            _output.WriteLine($"Array ToString: {sw1.ElapsedMilliseconds}ms");
            _output.WriteLine($"Span ToString: {sw2.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void VerifyAllOptimizationsProduceSameResults()
        {
            var testCases = new[]
            {
                "",
                "A",
                "ISO8583",
                "Financial Transaction Message",
                new string('X', 1000)
            };

            Span<sbyte> buffer = stackalloc sbyte[2048];

            foreach (var text in testCases)
            {
                var encoding = Encoding.UTF8;
                
                var standard = text.GetSignedBytes(encoding);
                
                var written = text.GetSignedBytes(buffer.Slice(0, Math.Max(1, text.GetSignedBytesCount(encoding))), encoding);
                var withBuffer = buffer.Slice(0, written).ToArray();
                
                var pooled = text.GetSignedBytesPooled(encoding);
                
                Assert.Equal(standard, withBuffer);
                Assert.Equal(standard, pooled);
                
                var roundTrip1 = standard.ToString(encoding);
                var roundTrip2 = withBuffer.ToString(encoding);
                var roundTrip3 = pooled.ToString(encoding);
                
                Assert.Equal(text, roundTrip1);
                Assert.Equal(text, roundTrip2);
                Assert.Equal(text, roundTrip3);
            }
        }

        [Fact]
        public void CompareMemoryAccess_RandomPatterns()
        {
            var random = new Random(42);
            var data = new byte[1000];
            random.NextBytes(data);

            const int iterations = 10000;
            
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var signed = data.ToInt8();
                var sum = 0;
                for (int j = 0; j < signed.Length; j++)
                {
                    sum += signed[j];
                }
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var signed = data.AsSignedBytes();
                var sum = 0;
                for (int j = 0; j < signed.Length; j++)
                {
                    sum += signed[j];
                }
            }
            sw2.Stop();

            _output.WriteLine($"Memory access old: {sw1.ElapsedMilliseconds}ms");
            _output.WriteLine($"Memory access new: {sw2.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void CompareSlicingOperations()
        {
            var data = new byte[1000];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }

            const int iterations = 10000;
            
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var signed = data.ToInt8();
                var slice = signed.Skip(100).Take(500).ToArray();
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var signed = data.AsSignedBytes();
                var slice = signed.Slice(100, 500).ToArray();
            }
            sw2.Stop();

            _output.WriteLine($"Slicing with LINQ: {sw1.ElapsedMilliseconds}ms");
            _output.WriteLine($"Slicing with Span: {sw2.ElapsedMilliseconds}ms");
            _output.WriteLine($"Speedup: {(double)sw1.ElapsedTicks / sw2.ElapsedTicks:F2}x");
        }
    }
}
