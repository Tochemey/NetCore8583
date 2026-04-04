// MIT License
//
// Copyright (c) 2020 - 2026 Arsene Tochemey Gandote
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetCore8583.Tracer;
using Xunit;

namespace NetCore8583.Test.Tracer
{
    public class TestSimpleTraceGenerator
    {
        [Fact]
        public void InitialValueIsReturned()
        {
            var gen = new SimpleTraceGenerator(1);
            Assert.Equal(0, gen.LastTrace);
            Assert.Equal(1, gen.NextTrace());
        }

        [Fact]
        public void InitialValueMaxIsReturned()
        {
            var gen = new SimpleTraceGenerator(999999);
            Assert.Equal(999999, gen.NextTrace());
        }

        [Fact]
        public void SequentialValues()
        {
            var gen = new SimpleTraceGenerator(10);
            Assert.Equal(10, gen.NextTrace());
            Assert.Equal(11, gen.NextTrace());
            Assert.Equal(12, gen.NextTrace());
            Assert.Equal(12, gen.LastTrace);
        }

        [Fact]
        public void WrapsAroundAt999999()
        {
            var gen = new SimpleTraceGenerator(999999);
            Assert.Equal(999999, gen.NextTrace());
            Assert.Equal(1, gen.NextTrace());
            Assert.Equal(1, gen.LastTrace);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000000)]
        [InlineData(-1)]
        public void OutOfRangeInitialValueThrows(int value)
        {
            Assert.Throws<ArgumentException>(() => new SimpleTraceGenerator(value));
        }

        [Fact]
        public async Task ThreadSafetyProducesUniqueValues()
        {
            var gen = new SimpleTraceGenerator(1);
            var results = new HashSet<int>();
            var lockObj = new object();
            var tasks = new Task[100];

            for (var i = 0; i < 100; i++)
                tasks[i] = Task.Run(() =>
                {
                    var val = gen.NextTrace();
                    lock (lockObj) results.Add(val);
                });

            await Task.WhenAll(tasks);
            Assert.Equal(100, results.Count);
        }
    }
}
