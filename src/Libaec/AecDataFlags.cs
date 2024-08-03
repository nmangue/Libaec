using System;

namespace Libaec;

/// <summary>
/// Sample data description flags
/// </summary>
[Flags]
public enum AecDataFlags
{
    AEC_DATA_NONE = 0,

    /// <summary>
    /// Samples are signed. Telling libaec this results in a slightly better compression ratio. Default is unsigned.
    /// </summary>
    AEC_DATA_SIGNED = 1,

    /// <summary>
    /// 17 to 24 bit samples are coded in 3 bytes
    /// </summary>
    AEC_DATA_3BYTE = 2,

    /// <summary>
    /// Samples are stored with their most significant bit first. This has nothing to do with the endianness of the host. Default is LSB.
    /// </summary>
    AEC_DATA_MSB = 4,

    /// <summary>
    /// Set if preprocessor should be used
    /// </summary>
    AEC_DATA_PREPROCESS = 8,

    /// <summary>
    /// Use restricted set of code options
    /// </summary>
    AEC_RESTRICTED = 16,

    /// <summary>
    /// Pad RSI to byte boundary. Only used for decoding some CCSDS sample data. Do not use this to produce new data as it violates the standard.
    /// </summary>
    AEC_PAD_RSI = 32,

    /// <summary>
    /// Do not enforce standard regarding legal block sizes.
    /// </summary>
    AEC_NOT_ENFORCE = 64
}
