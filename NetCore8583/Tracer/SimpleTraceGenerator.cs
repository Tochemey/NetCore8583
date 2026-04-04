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

namespace NetCore8583.Tracer
{
    /// <summary>Thread-safe trace number generator that cycles from 1 to 999999 (ISO 8583 field 11).</summary>
    public class SimpleTraceGenerator : ITraceNumberGenerator
    {
        private readonly object mutex = new object();
        private volatile int value;

        /// <summary>Creates a generator that will return <paramref name="initialValue"/> on the first call to <see cref="NextTrace"/>.</summary>
        /// <param name="initialValue">First trace value (1 to 999999).</param>
        /// <exception cref="ArgumentException">Thrown when initial value is out of range.</exception>
        public SimpleTraceGenerator(int initialValue)
        {
            if (initialValue < 1 || initialValue > 999999)
                throw new ArgumentException("Initial value must be between 1 and 999999");
            value = initialValue - 1;
        }

        /// <inheritdoc />
        public int LastTrace => value;

        /// <inheritdoc />
        public int NextTrace()
        {
            lock (mutex)
            {
                value++;
                if (value > 999999) value = 1;
            }

            return value;
        }
    }
}