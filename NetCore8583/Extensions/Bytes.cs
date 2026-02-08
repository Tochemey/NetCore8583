using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NetCore8583.Extensions
{
    /// <summary>
    ///     Utility class for byte array operations with zero-copy conversions
    ///     between signed and unsigned byte representations.
    /// </summary>
    public static class Bytes
    {
        /// <summary>
        ///     Returns a zero-copy <see cref="Span{T}"/> view of a byte array as signed bytes.
        ///     The returned span shares the same underlying memory as the source array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<sbyte> AsSignedBytes(this byte[] bytes)
        {
            return MemoryMarshal.Cast<byte, sbyte>(bytes.AsSpan());
        }

        /// <summary>
        ///     Returns a zero-copy <see cref="Span{T}"/> view of a byte span as signed bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<sbyte> AsSignedBytes(this Span<byte> bytes)
        {
            return MemoryMarshal.Cast<byte, sbyte>(bytes);
        }

        /// <summary>
        ///     Returns a zero-copy <see cref="ReadOnlySpan{T}"/> view of a byte span as signed bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<sbyte> AsSignedBytes(this ReadOnlySpan<byte> bytes)
        {
            return MemoryMarshal.Cast<byte, sbyte>(bytes);
        }

        /// <summary>
        ///     Returns a zero-copy <see cref="Span{T}"/> view of a signed byte array as unsigned bytes.
        ///     The returned span shares the same underlying memory as the source array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsUnsignedBytes(this sbyte[] bytes)
        {
            return MemoryMarshal.Cast<sbyte, byte>(bytes.AsSpan());
        }

        /// <summary>
        ///     Returns a zero-copy <see cref="Span{T}"/> view of a signed byte span as unsigned bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsUnsignedBytes(this Span<sbyte> bytes)
        {
            return MemoryMarshal.Cast<sbyte, byte>(bytes);
        }

        /// <summary>
        ///     Returns a zero-copy <see cref="ReadOnlySpan{T}"/> view of a signed byte span as unsigned bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsUnsignedBytes(this ReadOnlySpan<sbyte> bytes)
        {
            return MemoryMarshal.Cast<sbyte, byte>(bytes);
        }

        /// <summary>
        ///     Decodes a span of signed bytes into a string using the specified encoding.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(this ReadOnlySpan<sbyte> sbytes, Encoding encoding)
        {
            return encoding.GetString(MemoryMarshal.Cast<sbyte, byte>(sbytes));
        }

        /// <summary>
        ///     Decodes a slice of a signed byte span into a string using the specified encoding.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(this ReadOnlySpan<sbyte> sbytes, int pos, int len, Encoding encoding)
        {
            return encoding.GetString(MemoryMarshal.Cast<sbyte, byte>(sbytes.Slice(pos, len)));
        }

        /// <summary>
        ///     Creates a new signed byte array from an unsigned byte array.
        ///     Prefer <see cref="AsSignedBytes(byte[])"/> when a copy is not needed.
        /// </summary>
        public static sbyte[] ToInt8(this byte[] bytes)
        {
            var result = new sbyte[bytes.Length];
            bytes.AsSignedBytes().CopyTo(result);
            return result;
        }

        /// <summary>
        ///     Creates a new unsigned byte array from a signed byte array.
        ///     Prefer <see cref="AsUnsignedBytes(sbyte[])"/> when a copy is not needed.
        /// </summary>
        public static byte[] ToUint8(this sbyte[] bytes)
        {
            var result = new byte[bytes.Length];
            bytes.AsUnsignedBytes().CopyTo(result);
            return result;
        }

        /// <summary>
        ///     Converts a signed byte array to a string from a given position and length.
        /// </summary>
        public static string ToString(this sbyte[] sbytes, int pos, int len, Encoding encoding)
        {
            ReadOnlySpan<sbyte> span = sbytes;
            return span.ToString(pos, len, encoding);
        }

        /// <summary>
        ///     Converts a signed byte array to a string using the specified encoding.
        /// </summary>
        public static string ToString(this sbyte[] sbytes, Encoding encoding)
        {
            return ((ReadOnlySpan<sbyte>)sbytes).ToString(encoding);
        }
    }
}
