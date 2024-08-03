using System;
using System.Runtime.InteropServices;

namespace Libaec;

public static class Interop
{
    private const string LibraryName = "libaec";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_encode_init(ref AecStream strm);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_encode_enable_offsets(ref AecStream strm);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_encode_count_offsets(ref AecStream strm, out UIntPtr rsiOffsetsCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_encode_get_offsets(ref AecStream strm, [Out] UIntPtr[] rsiOffsets, UIntPtr rsiOffsetsCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_buffer_seek(ref AecStream strm, UIntPtr offset);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_encode(ref AecStream strm, int flush);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_encode_end(ref AecStream strm);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_decode_init(ref AecStream strm);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_decode_enable_offsets(ref AecStream strm);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_decode_count_offsets(ref AecStream strm, out UIntPtr rsiOffsetsCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_decode_get_offsets(ref AecStream strm, [Out] UIntPtr[] rsiOffsets, UIntPtr rsiOffsetsCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_decode(ref AecStream strm, int flush);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_decode_range(ref AecStream strm, [In] UIntPtr[] rsiOffsets, UIntPtr rsiOffsetsCount, UIntPtr pos, UIntPtr size);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_decode_end(ref AecStream strm);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_buffer_encode(ref AecStream strm);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int aec_buffer_decode(ref AecStream strm);
}
