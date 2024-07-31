using System.Globalization;
using System.Runtime.InteropServices;
using Xunit.Sdk;
using NFluent;

namespace Libaec.Tests
{
    public class UnitTest1
    {
        [Theory]
        [MemberData(nameof(AllOptionsTestData))]
        public void Decode_AllOptions_Test(string datFile, string rzFile, int nbSamples, int bitsPerSample, int rsi, AecDataFlags flags)
        {
            var expectedValues = DataSampleReader.ReadSamples(datFile, bitsPerSample);

            var compressedData = File.ReadAllBytes(rzFile);

            var nbBytesPerValue = DataSampleReader.CalculatePowerOfTwoBytesForBitSample(bitsPerSample);

            if (nbBytesPerValue == 4 && bitsPerSample <= 24 && flags.HasFlag(AecDataFlags.AEC_DATA_3BYTE))
            {
                nbBytesPerValue = 3;
            }

            var valuesBytes = new byte[nbSamples * nbBytesPerValue];

            // Pin the byte array in memory so that the garbage collector doesn't move it
            GCHandle compressedDataHandle = GCHandle.Alloc(compressedData, GCHandleType.Pinned);
            GCHandle valuesHandle = GCHandle.Alloc(valuesBytes, GCHandleType.Pinned);

            try
            {
                // Get the address of the pinned byte array as an IntPtr
                IntPtr dataPointer = compressedDataHandle.AddrOfPinnedObject();
                IntPtr valuesPointer = valuesHandle.AddrOfPinnedObject();

                AecStream strm = new AecStream
                {
                    BitsPerSample = (uint)bitsPerSample,
                    BlockSize = 16,
                    Rsi = (uint)rsi,
                    Flags = (uint)flags,
                    NextIn = dataPointer,
                    AvailIn = (uint)compressedData.Length,
                    NextOut = valuesPointer,
                    AvailOut = (uint)valuesBytes.Length
                };

                int result = Interop.aec_decode_init(ref strm);
                if (result != (int)AecReturnCode.AEC_OK)
                    throw new XunitException("Init failed");

                int result2 = Interop.aec_decode(ref strm, (int)AecFlushMode.AEC_FLUSH);
                if (result2 != (int)AecReturnCode.AEC_OK)
                    throw new XunitException("Decode failed");

                Interop.aec_decode_end(ref strm);
            }
            finally
            {
                compressedDataHandle.Free();
                valuesHandle.Free();
            }

            uint[] values;
            using (var ms  = new MemoryStream(valuesBytes))
            {
                values = DataSampleReader.ReadSamples(ms, nbBytesPerValue);
            }

            Check.That(values).ContainsExactly(expectedValues);

        }

        public static IEnumerable<object[]> AllOptionsTestData
        {
            get
            {
                var encodedFiles = Directory.GetFiles("./TestData/AllOptions", "*.rz");

                foreach (var rzFile in encodedFiles)
                {
                    string rzFileName = Path.GetFileNameWithoutExtension(rzFile);
                    var datFile = Path.Combine(Path.GetDirectoryName(rzFile)!, rzFileName.Substring(0, 12) + ".dat");

                    var flags = AecDataFlags.AEC_DATA_PREPROCESS;

                    if (rzFileName.Contains("restricted"))
                    {
                        flags |= AecDataFlags.AEC_RESTRICTED;
                    }

                    var nbSamples = int.Parse(rzFileName.Substring(6, 3), CultureInfo.InvariantCulture);
                    var bitsPerSample = int.Parse(rzFileName.Substring(10, 2), CultureInfo.InvariantCulture);

                    var rsi = bitsPerSample > 16 ? 32 : 16;

                    yield return new object[] { datFile, rzFile, nbSamples, bitsPerSample, rsi, flags };
                }
            }
        }
    }


    public static class DataSampleReader
    {
        public static uint[] ReadSamples(string filePath, int dynamicRange)
        {
            int bytesPerSample = GetBytesPerSample(dynamicRange);
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return ReadSamples(fileStream, bytesPerSample);
        }

        public static uint[] ReadSamples(Stream input, int bytesPerSample)
        {
            var samples = new List<uint>();

            using (var reader = new BinaryReader(input))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    samples.Add(ReadSample(reader, bytesPerSample));
                }
            }

            return [.. samples];
        }

        private static int GetBytesPerSample(int dynamicRange) =>
            dynamicRange <= 8 ? 1 : dynamicRange <= 16 ? 2 : 4;

        private static uint ReadSample(BinaryReader reader, int bytesPerSample)
        {
            byte[] bytes = reader.ReadBytes(bytesPerSample);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytesPerSample switch
            {
                1 => bytes[0],
                2 => BitConverter.ToUInt16(bytes, 0),
                3 => (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16)),
                4 => BitConverter.ToUInt32(bytes, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(bytesPerSample), "Invalid bytes per sample.")
            };
        }

        /// <summary>
        /// Calculates the number of bytes needed to store a given number of bits, 
        /// rounding up to the nearest power of 2.
        /// </summary>
        /// <param name="bitsPerSample">The number of bits to be stored. Must be a positive integer.</param>
        /// <returns>
        /// The smallest power of 2 number of bytes that can contain the specified number of bits.
        /// </returns>
        /// <remarks>
        /// This method uses bitwise operations for efficiency. It first calculates the minimum
        /// number of bytes needed to store the bits, then rounds this up to the next power of 2.
        /// 
        /// The steps are as follows:
        /// 1. Calculate the minimum bytes needed: (bitsPerSample + 7) / 8
        /// 2. Find the next power of 2 using bitwise operations
        /// 
        /// This method is optimized for performance and is suitable for scenarios where
        /// memory allocation needs to be in power-of-2 blocks.
        /// 
        /// Examples:
        /// - For 1-8 bits, it returns 1 byte
        /// - For 9-16 bits, it returns 2 bytes
        /// - For 17-32 bits, it returns 4 bytes
        /// - For 33-64 bits, it returns 8 bytes
        /// 
        /// Note: This method assumes that the input will not cause an integer overflow.
        /// For very large bit counts (> 2^28), consider using a long integer version.
        /// </remarks>
        public static int CalculatePowerOfTwoBytesForBitSample(int bitsPerSample)
        {
            int bytesNeeded = (bitsPerSample + 7) >> 3;  // Equivalent to (bitsPerSample + 7) / 8
            bytesNeeded--;
            bytesNeeded |= bytesNeeded >> 1;
            bytesNeeded |= bytesNeeded >> 2;
            bytesNeeded |= bytesNeeded >> 4;
            bytesNeeded |= bytesNeeded >> 8;
            bytesNeeded |= bytesNeeded >> 16;
            bytesNeeded++;
            return bytesNeeded;
        }

    }

}