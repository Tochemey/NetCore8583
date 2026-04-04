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

namespace NetCore8583.Codecs
{
    /// <summary>
    /// Custom field encoder/decoder for LLBIN/LLLBIN fields that contain long integers in BCD encoding.
    /// </summary>
    public class LongBcdCodec : ICustomBinaryField
    {
        /// <inheritdoc />
        public object DecodeField(string val) => long.Parse(val);

        /// <inheritdoc />
        public string EncodeField(object obj) => Convert.ToString(obj);

        /// <inheritdoc />
        public object DecodeBinaryField(sbyte[] bytes, int offset, int length) =>
            Bcd.DecodeToLong(bytes, offset, length * 2);

        /// <inheritdoc />
        public sbyte[] EncodeBinaryField(object obj)
        {
            var s = Convert.ToString(obj);
            if (s == null) return null;
            var buf = new sbyte[s.Length / 2 + s.Length % 2];
            Bcd.Encode(s, buf);
            return buf;
        }
    }
}