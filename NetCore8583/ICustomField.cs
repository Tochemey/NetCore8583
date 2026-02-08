namespace NetCore8583
{
    /// <summary>Interface for custom encoding/decoding of ISO 8583 field values (e.g. special formats, encryption).</summary>
    public interface ICustomField
    {
        /// <summary>
        ///     Decodes a custom field value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        object DecodeField(string value);

        /// <summary>
        ///     Encode a custom field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string EncodeField(object value);
    }
}