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

using System.Text;
using NetCore8583.Extensions;
using Xunit;

namespace NetCore8583.Test.Extensions
{
    public class TestEncodingCache
    {
        [Fact]
        public void Utf8IsUtf8Encoding()
        {
            Assert.NotNull(EncodingCache.Utf8);
            Assert.Equal(Encoding.UTF8, EncodingCache.Utf8);
        }

        [Fact]
        public void DefaultIsDefaultEncoding()
        {
            Assert.NotNull(EncodingCache.Default);
            Assert.Equal(Encoding.Default, EncodingCache.Default);
        }

        [Fact]
        public void AsciiIsAsciiEncoding()
        {
            Assert.NotNull(EncodingCache.Ascii);
            Assert.Equal(Encoding.ASCII, EncodingCache.Ascii);
        }

        [Fact]
        public void UnicodeIsUnicodeEncoding()
        {
            Assert.NotNull(EncodingCache.Unicode);
            Assert.Equal(Encoding.Unicode, EncodingCache.Unicode);
        }

        [Fact]
        public void Utf32IsUtf32Encoding()
        {
            Assert.NotNull(EncodingCache.Utf32);
            Assert.Equal(Encoding.UTF32, EncodingCache.Utf32);
        }
    }
}
