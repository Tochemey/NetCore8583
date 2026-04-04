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

namespace NetCore8583.Extensions
{
    /// <summary>
    ///     Provides cached encoding instances to avoid repeated allocations.
    ///     Encoding objects are thread-safe and reusable.
    /// </summary>
    public static class EncodingCache
    {
        /// <summary>
        ///     Cached UTF-8 encoding instance (same as Encoding.UTF8 but explicit).
        /// </summary>
        public static readonly Encoding Utf8 = Encoding.UTF8;

        /// <summary>
        ///     Cached Default encoding instance (platform default).
        /// </summary>
        public static readonly Encoding Default = Encoding.Default;

        /// <summary>
        ///     Cached ASCII encoding instance.
        /// </summary>
        public static readonly Encoding Ascii = Encoding.ASCII;

        /// <summary>
        ///     Cached UTF-16 (Unicode) encoding instance.
        /// </summary>
        public static readonly Encoding Unicode = Encoding.Unicode;

        /// <summary>
        ///     Cached UTF-32 encoding instance.
        /// </summary>
        public static readonly Encoding Utf32 = Encoding.UTF32;
    }
}
