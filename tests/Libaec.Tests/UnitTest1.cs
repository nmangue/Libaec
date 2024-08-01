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

            var decoder = new AecDecoder(bitsPerSample, flags, 16, rsi);

            var values = decoder.Decode(compressedData, compressedData.Length, nbSamples);

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
    }

}