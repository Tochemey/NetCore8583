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

using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestOsUtil
    {
        [Fact]
        public void IsLinuxReturnsBoolWithoutThrowing()
        {
            // Just verify it runs and returns a bool without throwing.
            // The value depends on the platform running the test.
            var result = OsUtil.IsLinux();
            Assert.IsType<bool>(result);
        }

        [Fact]
        public void IsLinuxMatchesPlatform()
        {
            // OsUtil.IsLinux() returns true for any Unix-like platform (Linux, macOS)
            // by checking Environment.OSVersion.Platform against Unix platform IDs (4, 6, 128).
            var p = (int) System.Environment.OSVersion.Platform;
            var expectedUnixLike = p is 4 or 6 or 128;
            Assert.Equal(expectedUnixLike, OsUtil.IsLinux());
        }
    }
}
