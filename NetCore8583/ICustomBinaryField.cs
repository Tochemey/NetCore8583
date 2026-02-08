namespace NetCore8583
{
    /// <summary>Extends <see cref="ICustomField"/> for fields that are transmitted as raw binary (e.g. LLBIN, LLLBIN) with encode/decode of byte buffers.</summary>
    public interface ICustomBinaryField : ICustomField
    {
        /// <summary>
        ///     Decode a custom binary field
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        object DecodeBinaryField(sbyte[] bytes, int offset, int length);

        /// <summary>
        ///     Encode a custom binary field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        sbyte[] EncodeBinaryField(object value);
    }
}