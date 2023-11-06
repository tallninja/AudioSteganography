using System.Text;

namespace AudioSteganography;

public class Decoder
{
    public static byte[] Decode(AudioData encodedData)
    {
        if (encodedData is null) throw new ArgumentNullException(nameof(encodedData));

        // extract the encoded bytes
        var (fileSize, offset) = ExtractFileSize(encodedData.DataSubchunk.Data, 0);
        // Console.WriteLine($"[INFO] file size: {fileSize}");
        return ExtractBytes(encodedData.DataSubchunk.Data, offset, fileSize);
    }

    public static byte[] ExtractBytes(byte[] encodedData, int offset, int endIndex)
    {
        // Console.WriteLine("[INFO] Extracting bytes...");

        var extractedBytes = new List<byte>();

        for (var i = 0; i < endIndex; i++)
        {
            var (byt, _offset) = ExtractByte(encodedData, offset);
            extractedBytes.Add(byt);
            offset += _offset;
        }


        return extractedBytes.ToArray();
    }

    public static (int fileSize, int offset) ExtractFileSize(byte[] encodedData, int offset = 0)
    {
        var fileSize = 0;
        var shift = 0;
        var _offset = offset;

        for (var i = 0; i < 4; i++)
        {
            var (byt, inc) = ExtractByte(encodedData, _offset);
            fileSize |= byt << shift;
            shift += 8;
            _offset += inc;
        }

        return (fileSize, _offset);
    }

    public static ( byte byt, int offset ) ExtractByte(byte[] encodedData, int offset = 0)
    {
        byte byt = 0;
        var shift = 0;

        for (var i = 0; i < 4; i++)
        {
            byt = (byte) (byt | ((encodedData[offset + i] & 3) << shift));
            shift += 2;
        }

        return (byt, 4);
    }
}