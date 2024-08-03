using System;
using System.Runtime.InteropServices;

namespace Libaec;

/// <summary>
/// Represents the AEC (Adaptive Entropy Coder) stream structure for PInvoke.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct AecStream
{
    /// <summary>
    /// Pointer to the next input byte.
    /// </summary>
    public IntPtr NextIn;

    /// <summary>
    /// Number of bytes available at NextIn.
    /// </summary>
    public UIntPtr AvailIn;

    /// <summary>
    /// Total number of input bytes read so far.
    /// </summary>
    public UIntPtr TotalIn;

    /// <summary>
    /// Pointer to the next output byte.
    /// </summary>
    public IntPtr NextOut;

    /// <summary>
    /// Remaining free space at NextOut.
    /// </summary>
    public UIntPtr AvailOut;

    /// <summary>
    /// Total number of bytes output so far.
    /// </summary>
    public UIntPtr TotalOut;

    /// <summary>
    /// Resolution in bits per sample (n = 1, ..., 32).
    /// </summary>
    public uint BitsPerSample;

    /// <summary>
    /// Block size in samples.
    /// </summary>
    public uint BlockSize;

    /// <summary>
    /// Reference sample interval, the number of blocks between consecutive reference samples (up to 4096).
    /// </summary>
    public uint Rsi;

    /// <summary>
    /// Flags for the AEC stream.
    /// </summary>
    public uint Flags;

    /// <summary>
    /// Pointer to the internal state of the AEC stream.
    /// </summary>
    public IntPtr State;
}
