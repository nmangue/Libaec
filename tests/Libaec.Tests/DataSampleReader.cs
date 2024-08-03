namespace Libaec.Tests;

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