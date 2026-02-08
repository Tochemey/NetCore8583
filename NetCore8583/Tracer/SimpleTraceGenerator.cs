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