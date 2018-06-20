namespace NetCore8583
{
    /// <summary>
    ///     Defines the behavior needed to generate trace numbers. They have to
    ///     messages. It must provide sequence numbers between 1 and 999999, as per the ISO standard.
    ///     This value is put in field 11.
    ///     A default version that simply iterates through an int in memory is provided.
    /// </summary>
    public interface ITraceNumberGenerator
    {
        /// <summary>
        ///     Returns the last trace number that was returned by the NextTrace()
        ///     method.
        /// </summary>
        int LastTrace { get; }

        /// <summary>
        ///     Returns the next trace number to be used in a message.
        /// </summary>
        int NextTrace();
    }
}