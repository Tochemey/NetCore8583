using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NetCore8583.Extensions
{
    /// <summary>
    ///     String utility extensions for emptiness checks and byte encoding.
    /// </summary>
    public static class Stringx
    {
        /// <summary>
        ///     Returns true when the string is null or empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this string string0)
        {
            return string.IsNullOrEmpty(string0);
        }

        /// <summary>
        ///     Converts a string to a UTF-8 byte array.
        /// </summary>
        public static byte[] GetBytes(this string check)
        {
            return Encoding.UTF8.GetBytes(check);
        }

        /// <summary>
        ///     Converts a string to a signed byte array using the specified encoding.
        ///     Encodes directly into a reinterpreted buffer so only one array is allocated.
        /// </summary>
        /// <param name="check">The string to convert.</param>
        /// <param name="encoding">The encoding to use. Defaults to <see cref="Encoding.Default"/>.</param>
        /// <returns>A new signed byte array containing the encoded bytes.</returns>
        public static sbyte[] GetSignedBytes(this string check, Encoding encoding = null)
        {
            encoding ??= Encoding.Default;
            var byteCount = encoding.GetByteCount(check);
            var result = new sbyte[byteCount];
            encoding.GetBytes(check, MemoryMarshal.Cast<sbyte, byte>(result.AsSpan()));
            return result;
        }

        /// <summary>
        ///     Tries to encode a string into a caller-provided signed byte buffer.
        ///     Returns false if the buffer is too small.
        /// </summary>
        /// <param name="check">The string to encode.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="bytesWritten">The number of bytes written on success.</param>
        /// <param name="encoding">The encoding to use. Defaults to <see cref="Encoding.Default"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetSignedBytes(this string check, Span<sbyte> destination,
            out int bytesWritten, Encoding encoding = null)
        {
            encoding ??= Encoding.Default;
            if (encoding.TryGetBytes(check, MemoryMarshal.Cast<sbyte, byte>(destination), out bytesWritten))
                return true;

            bytesWritten = 0;
            return false;
        }

        /// <summary>
        ///     Encodes a string into a caller-provided signed byte buffer.
        ///     The caller must ensure the buffer is large enough.
        /// </summary>
        /// <param name="check">The string to encode.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="encoding">The encoding to use. Defaults to <see cref="Encoding.Default"/>.</param>
        /// <returns>The number of bytes written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSignedBytes(this string check, Span<sbyte> destination,
            Encoding encoding = null)
        {
            encoding ??= Encoding.Default;
            return encoding.GetBytes(check, MemoryMarshal.Cast<sbyte, byte>(destination));
        }

        /// <summary>
        ///     Returns the number of bytes required to encode the string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSignedBytesCount(this string check, Encoding encoding = null)
        {
            encoding ??= Encoding.Default;
            return encoding.GetByteCount(check);
        }

        /// <summary>
        ///     Encodes a string to a signed byte array, using an <see cref="ArrayPool{T}"/>
        ///     buffer internally to reduce intermediate allocations.
        /// </summary>
        public static sbyte[] GetSignedBytesPooled(this string check, Encoding encoding = null)
        {
            encoding ??= Encoding.Default;
            var byteCount = encoding.GetByteCount(check);
            var pooledBuffer = ArrayPool<sbyte>.Shared.Rent(byteCount);
            try
            {
                var written = check.GetSignedBytes(pooledBuffer.AsSpan(), encoding);
                var result = new sbyte[written];
                pooledBuffer.AsSpan(0, written).CopyTo(result);
                return result;
            }
            finally
            {
                ArrayPool<sbyte>.Shared.Return(pooledBuffer);
            }
        }
    }
}
