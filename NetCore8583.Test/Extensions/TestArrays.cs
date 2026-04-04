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
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestArrays
    {
        [Fact]
        public void FillSetsValuesInRange()
        {
            var arr = new int[5];
            Arrays.Fill(arr, 1, 3, 99);
            Assert.Equal(new[] { 0, 99, 99, 99, 0 }, arr);
        }

        [Fact]
        public void FillFromZero()
        {
            var arr = new string[4];
            Arrays.Fill(arr, 0, 3, "x");
            Assert.Equal(new[] { "x", "x", "x", null }, arr);
        }

        [Fact]
        public void FillNullArrayThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Arrays.Fill<int>(null, 0, 1, 0));
        }

        [Fact]
        public void FillNegativeCountThrows()
        {
            var arr = new int[5];
            Assert.Throws<ArgumentOutOfRangeException>(() => Arrays.Fill(arr, 0, -1, 0));
        }

        [Fact]
        public void FillOutOfRangeCountThrows()
        {
            var arr = new int[5];
            Assert.Throws<ArgumentOutOfRangeException>(() => Arrays.Fill(arr, 0, 5, 0));
        }
    }
}
