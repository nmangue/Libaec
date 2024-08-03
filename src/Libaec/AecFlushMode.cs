namespace Libaec;

/// <summary>
/// Options for flushing
/// </summary>
public enum AecFlushMode
{
    /// <summary>
    /// Do not enforce output flushing. More input may be provided with later calls. So far only relevant for encoding.
    /// </summary>
    AEC_NO_FLUSH = 0,

    /// <summary>
    /// Flush output and end encoding. The last call to aec_encode() must set AEC_FLUSH to drain all output.
    /// It is not possible to continue encoding of the same stream after it has been flushed.
    /// </summary>
    AEC_FLUSH = 1
}
