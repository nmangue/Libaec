using System.Globalization;
using NFluent;

namespace Libaec.Tests;

public class AecDecoderTests
{
    [Theory]
    [MemberData(nameof(AllOptionsTestData))]
    public void Decode_AllOptions_Test(string datFile, string rzFile, int nbSamples, int bitsPerSample, int rsi, AecDataFlags flags)
    {
        CheckDecode(datFile, rzFile, nbSamples, bitsPerSample, rsi, 16, flags);
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

    [Theory]
    [MemberData(nameof(LowEntropyOptions))]
    public void Decode_LowEntropyOptions_Test(string datFile, string rzFile, int nbSamples, int bitsPerSample, int rsi, AecDataFlags flags)
    {
        CheckDecode(datFile, rzFile, nbSamples, bitsPerSample, rsi, 16, flags);
    }

    public static IEnumerable<object[]> LowEntropyOptions
    {
        get
        {
            var encodedFiles = Directory.GetFiles("./TestData/LowEntropyOptions", "*.rz");

            foreach (var rzFile in encodedFiles)
            {
                string rzFileName = Path.GetFileNameWithoutExtension(rzFile);
                var datFile = Path.Combine(Path.GetDirectoryName(rzFile)!, rzFileName.Substring(0, 12) + ".dat");

                var flags = AecDataFlags.AEC_DATA_PREPROCESS;

                if (rzFileName.Contains("restricted"))
                {
                    flags |= AecDataFlags.AEC_RESTRICTED;
                }

                var i = int.Parse(rzFileName.Substring(6, 1), CultureInfo.InvariantCulture);

                var nbSamples = i == 1 ? 432 : i == 2 ? 1024 : 2048;

                var bitsPerSample =int.Parse(rzFileName.Substring(14, 2), CultureInfo.InvariantCulture);

                var rsi = 64;

                yield return new object[] { datFile, rzFile, nbSamples, bitsPerSample, rsi, flags };
            }
        }
    }

    [Theory]
    [MemberData(nameof(ExtendedParameters))]
    public void Decode_ExtendedParameters_Test(string datFile, string rzFile, int nbSamples, int bitsPerSample, int rsi, int blockSize, AecDataFlags flags)
    {
        CheckDecode(datFile, rzFile, nbSamples, bitsPerSample, rsi, blockSize, flags);
    }

    public static IEnumerable<object[]> ExtendedParameters
    {
        get
        {
            var encodedFiles = Directory.GetFiles("./TestData/ExtendedParameters", "*.rz");

            foreach (var rzFile in encodedFiles)
            {
                string rzFileName = Path.GetFileNameWithoutExtension(rzFile);
                var datFile = Path.Combine(Path.GetDirectoryName(rzFile)!, rzFileName.Substring(0, 8) + ".dat");

                var flags = AecDataFlags.AEC_DATA_PREPROCESS | AecDataFlags.AEC_PAD_RSI;

                var bitsPerSample = int.Parse(rzFileName.Substring(3, 2), CultureInfo.InvariantCulture);

                var j = int.Parse(rzFileName.Substring(10, 2), CultureInfo.InvariantCulture);

                var rsi = int.Parse(rzFileName.Substring(14), CultureInfo.InvariantCulture);

                var nbSamples = 512 * 512;

                yield return new object[] { datFile, rzFile, nbSamples, bitsPerSample, rsi, j, flags };
            }
        }
    }

    private static void CheckDecode(string datFile, string rzFile, int nbSamples, int bitsPerSample, int rsi, int blockSize, AecDataFlags flags)
    {
        var expectedValues = DataSampleReader.ReadSamples(datFile, bitsPerSample);

        var compressedData = File.ReadAllBytes(rzFile);

        var decoder = new AecDecoder(bitsPerSample, flags, blockSize, rsi);

        var values = decoder.Decode(compressedData, compressedData.Length, nbSamples);

        Check.That(values).ContainsExactly(expectedValues);
    }
}
