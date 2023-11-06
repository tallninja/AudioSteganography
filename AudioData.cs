using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioSteganography;

/// <summary>
/// This is an assembly of the WAVE file audio data
/// </summary>
public class AudioData
{
    public AudioData(RiffChunk riffChunk, FmtSubchunk fmtSubchunk, DataSubchunk dataSubchunk)
    {
        RiffChunk = riffChunk;
        FmtSubchunk = fmtSubchunk;
        DataSubchunk = dataSubchunk;
    }

    public RiffChunk RiffChunk { get; }
    public FmtSubchunk FmtSubchunk { get; }
    public DataSubchunk DataSubchunk { get; }

    public string Save(string filePath)
    {
        var fullPath = Path.Join(Directory.GetCurrentDirectory(), filePath);
        var outFile = File.Open(fullPath, FileMode.Create);
        using var writer = new BinaryWriter(outFile);
        writer.Write(Build(this));
        return fullPath;
    }

    public static byte[] Build(AudioData audioData)
    {
        var riffChunk = audioData.RiffChunk.Build();
        var fmtSubchunk = audioData.FmtSubchunk.Build();
        var dataSubChunk = audioData.DataSubchunk.Build();

        return CombineArrays(riffChunk, fmtSubchunk, dataSubChunk);

        byte[] CombineArrays(params byte[][] byteArrays) =>
            byteArrays.SelectMany(arrays => arrays).ToArray();
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

/// <summary>
/// The canonical WAVE format starts with the RIFF header
/// </summary>
public struct RiffChunk
{
    public RiffChunk(string chunkId, int chunkSize, string format)
    {
        ChunkId = chunkId;
        ChunkSize = chunkSize;
        Format = format;
    }

    /// <summary>
    /// Contains the letters "RIFF" in ASCII form
    /// (0x52494646 big-endian form).
    /// </summary>
    public string ChunkId { get; }

    /// <summary>
    /// 36 + SubChunk2Size, or more precisely:
    /// 4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
    /// This is the size of the rest of the chunk
    /// following this number.  This is the size of the
    /// entire file in bytes minus 8 bytes for the
    /// two fields not included in this count:
    /// ChunkID and ChunkSize.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Contains the letters "WAVE"
    /// (0x57415645 big-endian form)
    /// </summary>
    public string Format { get; }

    public byte[] Build()
    {
        var chunkId = Encoding.UTF8.GetBytes(ChunkId);
        var chunkSize = BitConverter.GetBytes(ChunkSize);
        var format = Encoding.UTF8.GetBytes(Format);

        return CombineArrays(chunkId, chunkSize, format);

        byte[] CombineArrays(params byte[][] byteArrays) =>
            byteArrays.SelectMany(arrays => arrays).ToArray();
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

/// <summary>
/// The "fmt " subchunk describes the sound data's format:
/// </summary>
public struct FmtSubchunk
{
    public FmtSubchunk(
        string subchunk1Id, int subchunk1Size,
        short audioFormat, short numChannels,
        int sampleRate, int byteRate,
        short blockAlign, short bitsPerSample)
    {
        Subchunk1Id = subchunk1Id;
        SubchunkSize = subchunk1Size;
        AudioFormat = audioFormat;
        NumChannels = numChannels;
        SampleRate = sampleRate;
        ByteRate = byteRate;
        BlockAlign = blockAlign;
        BitsPerSample = bitsPerSample;
    }

    /// <summary>
    /// Contains the letters <c>"fmt "</c>
    /// (0x666d7420 big-endian form).
    /// </summary>
    public string Subchunk1Id { get; }

    /// <summary>
    /// 16 for PCM.  This is the size of the
    /// rest of the Subchunk which follows this number.
    /// </summary>
    public int SubchunkSize { get; }

    /// <summary>
    /// PCM = 1 (i.e. Linear quantization)
    /// Values other than 1 indicate some
    /// form of compression.
    /// </summary>
    public short AudioFormat { get; }

    /// <summary>
    /// Mono = 1, Stereo = 2, etc.
    /// </summary>
    public short NumChannels { get; }

    /// <summary>
    /// 8000, 44100, etc.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// SampleRate * NumChannels * BitsPerSample/8
    /// </summary>
    public int ByteRate { get; }

    /// <summary>
    /// NumChannels * BitsPerSample/8
    /// The number of bytes for one sample including
    /// all channels. I wonder what happens when
    /// this number isn't an integer?
    /// </summary>
    public short BlockAlign { get; }

    /// <summary>
    /// 8 bits = 8, 16 bits = 16, etc.
    /// </summary>
    public short BitsPerSample { get; }

    public byte[] Build()
    {
        var subchunk1Id = Encoding.UTF8.GetBytes(Subchunk1Id);
        var subchunkSize = BitConverter.GetBytes(SubchunkSize);
        var audioFormat = BitConverter.GetBytes(AudioFormat);
        var numChannels = BitConverter.GetBytes(NumChannels);
        var sampleRate = BitConverter.GetBytes(SampleRate);
        var byteRate = BitConverter.GetBytes(ByteRate);
        var blockAlign = BitConverter.GetBytes(BlockAlign);
        var bitsPerSample = BitConverter.GetBytes(BitsPerSample);

        return CombineArrays(subchunk1Id, subchunkSize, audioFormat, numChannels,
            sampleRate, byteRate, blockAlign, bitsPerSample);

        byte[] CombineArrays(params byte[][] byteArrays) =>
            byteArrays.SelectMany(arrays => arrays).ToArray();
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}


/// <summary>
/// The "data" subchunk contains the size of the data and the actual sound:
/// </summary>
public struct DataSubchunk
{
    public DataSubchunk(string subchunk2Id, int subchunk2Size, byte[] data)
    {
        Subchunk2Id = subchunk2Id;
        Subchunk2Size = subchunk2Size;
        Data = data;
    }

    /// <summary>
    /// Contains the letters "data"
    /// (0x64617461 big-endian form).
    /// </summary>
    public string Subchunk2Id { get; }

    /// <summary>
    /// NumSamples * NumChannels * BitsPerSample/8
    /// This is the number of bytes in the data.
    /// You can also think of this as the size
    /// of the read of the subchunk following this
    /// number.
    /// </summary>
    public int Subchunk2Size { get; }

    /// <summary>
    /// The actual sound data.
    /// </summary>
    [JsonIgnore]
    public byte[] Data { get; }

    public byte[] Build()
    {
        var subchunk2Id = Encoding.UTF8.GetBytes(Subchunk2Id);
        var subchunk2Size = BitConverter.GetBytes(Subchunk2Size);

        return CombineArrays(subchunk2Id, subchunk2Size, Data);

        byte[] CombineArrays(params byte[][] byteArrays) =>
            byteArrays.SelectMany(arrays => arrays).ToArray();
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}