namespace Libaec;

/// <summary>
/// Return codes of library functions
/// </summary>
public enum AecReturnCode
{
    AEC_OK = 0,
    AEC_CONF_ERROR = -1,
    AEC_STREAM_ERROR = -2,
    AEC_DATA_ERROR = -3,
    AEC_MEM_ERROR = -4,
    AEC_RSI_OFFSETS_ERROR = -5
}
