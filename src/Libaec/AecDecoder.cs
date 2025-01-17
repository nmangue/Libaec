﻿using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace Libaec;

public class AecDecoder
{
    private readonly int bitsPerSample;
    private readonly AecDataFlags flags;
    private readonly int blockSize;
    private readonly int rsi;

    public AecDecoder(int bitsPerSample, AecDataFlags flags, int blockSize, int rsi)
    {
        this.bitsPerSample = bitsPerSample;
        this.flags = flags;
        this.blockSize = blockSize;
        this.rsi = rsi;
    }

    public uint[] Decode(byte[] compressedData, int compressedLength, int nbSamples)
    {
        var nbBytesPerSample = GetDecompressedSampleByteSize(bitsPerSample);

        int decompressedLength = nbSamples * nbBytesPerSample;
        var decompressedData = ArrayPool<byte>.Shared.Rent(decompressedLength);

        // Pin the byte array in memory so that the garbage collector doesn't move it
        GCHandle compressedDataHandle = GCHandle.Alloc(compressedData, GCHandleType.Pinned);
        GCHandle valuesHandle = GCHandle.Alloc(decompressedData, GCHandleType.Pinned);

        try
        {
            // Get the address of the pinned byte array as an IntPtr
            IntPtr dataPointer = compressedDataHandle.AddrOfPinnedObject();
            IntPtr valuesPointer = valuesHandle.AddrOfPinnedObject();

            AecStream strm = new()
            {
                BitsPerSample = (uint)bitsPerSample,
                BlockSize = (uint)blockSize,
                Rsi = (uint)rsi,
                Flags = (uint)flags,
                NextIn = dataPointer,
                AvailIn = (UIntPtr)compressedLength,
                NextOut = valuesPointer,
                AvailOut = (UIntPtr)decompressedLength
            };

            int aecReturn = Interop.aec_decode_init(ref strm);
            if (aecReturn != (int)AecReturnCode.AEC_OK)
            {
                throw new AecException("Decode init failed", aecReturn);
            }

            aecReturn = Interop.aec_decode(ref strm, (int)AecFlushMode.AEC_FLUSH);
            if (aecReturn != (int)AecReturnCode.AEC_OK)
            {
                throw new AecException("Decode failed", aecReturn);
            }

            Interop.aec_decode_end(ref strm);
        }
        finally
        {
            compressedDataHandle.Free();
            valuesHandle.Free();
        }

        var values = new uint[nbSamples];

        using (var ms = new MemoryStream(decompressedData))
        using (var reader = new BinaryReader(ms))
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = ReadSample(reader, nbBytesPerSample);
            }
        }

        ArrayPool<byte>.Shared.Return(decompressedData);

        return values;
    }


    /// <summary>
    /// Calculates the number of bytes needed to store a given number of bits.
    /// </summary>
    /// <param name="bitsPerSample">The number of bits to be stored. Must be a positive integer.</param>
    /// <returns>
    /// The smallest number of bytes that can contain the specified number of bits.
    /// </returns>
    /// <remarks>
    /// Examples:
    /// - For 1-8 bits, it returns 1 byte
    /// - For 9-16 bits, it returns 2 bytes
    /// - For 17-32 bits, it returns 4 bytes or 3 bytes if the AEC_DATA_3BYTE flag is set
    /// - For 33-64 bits, it returns 8 bytes
    /// </remarks>
    public int GetDecompressedSampleByteSize(int bitsPerSample)
    {
        // This method uses bitwise operations for efficiency. It first calculates the minimum
        // number of bytes needed to store the bits, then rounds this up to the next power of 2.
        // 
        // The steps are as follows:
        // 1. Calculate the minimum bytes needed: (bitsPerSample + 7) / 8
        // 2. Find the next power of 2 using bitwise operations
        int bytesNeeded = (bitsPerSample + 7) >> 3;  // Equivalent to (bitsPerSample + 7) / 8
        bytesNeeded--;
        bytesNeeded |= bytesNeeded >> 1;
        bytesNeeded |= bytesNeeded >> 2;
        bytesNeeded |= bytesNeeded >> 4;
        bytesNeeded |= bytesNeeded >> 8;
        bytesNeeded |= bytesNeeded >> 16;
        bytesNeeded++;

        if (bytesNeeded == 4 && bitsPerSample <= 24 && flags.HasFlag(AecDataFlags.AEC_DATA_3BYTE))
        {
            bytesNeeded = 3;
        }

        return bytesNeeded;
    }

    private static uint ReadSample(BinaryReader reader, int bytesPerSample)
    {
        return bytesPerSample switch
        {
            1 => reader.ReadByte(),
            2 => reader.ReadUInt16(),
            3 => (uint)(reader.ReadByte() | (reader.ReadByte() << 8) | (reader.ReadByte() << 16)),
            4 => reader.ReadUInt32(),
            _ => throw new ArgumentOutOfRangeException(nameof(bytesPerSample), "Invalid bytes per sample.")
        };
    }
}
