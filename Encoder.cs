using System.Text;

namespace AudioSteganography;

public class Encoder
{
    public static byte[] Encode(AudioData audioData, byte[] data)
    {
        var (_, dataSize) = CheckAvailableSpace(audioData.DataSubchunk.Data, data);
        var offset = EncodeBytes(BitConverter.GetBytes(dataSize), audioData.DataSubchunk.Data, 0);
        _ = EncodeBytes(data, audioData.DataSubchunk.Data, offset);

        return AudioData.Build(audioData);
    }

    private static (int availableSpace, int dataSize) CheckAvailableSpace(byte[] soundData, byte[] data)
    {
        if (soundData == null) throw new ArgumentNullException(nameof(soundData));
        if (data == null) throw new ArgumentNullException(nameof(data));

        var availableSpace = soundData.Length / 4;
        var dataSize = data.Length;

        Console.WriteLine($"[INFO] data size: {dataSize}");

        if (availableSpace < data.Length)
        {
            throw new Exception("Not enough space for data encoding.");
        }

        return (availableSpace, dataSize);
    }

    private static int EncodeBytes(byte[] bytesToEncode, byte[] audioBytes, int offset)
    {
        if (bytesToEncode is null) throw new ArgumentNullException(nameof(bytesToEncode));
        if (audioBytes == null) throw new ArgumentNullException(nameof(audioBytes));

        return bytesToEncode
            .Aggregate(offset,
                (current, byt) =>
                    current + EncodeByte(byt, audioBytes, current));
    }

    private static int EncodeByte(byte byt, byte[] soundData, int offset)
    {
        if (soundData is null) throw new ArgumentNullException(nameof(soundData));

        for (var i = 0; i < 4; i++)
        {
            soundData[offset + i] = (byte) ((soundData[offset + i] & 0xfffc) | (byt & 3));
            byt >>= 2;
        }

        return 4;
    }
}