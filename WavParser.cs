using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioSteganography;

/// <summary>
/// Parses a WAVE file
/// Resources: <see href="http://soundfile.sapp.org/doc/WaveFormat/" keyword="WAVE PCM soundfile format"/>
/// </summary>
public abstract class WavParser
{
    public static AudioData Parse(string filePath)
    {
        if (filePath is null) throw new ArgumentNullException(nameof(filePath));

        using var file = File.Open(filePath, FileMode.Open);
        var reader = new BinaryReader(file);
        var riffChunk = ParseRiffChunk(reader);
        var fmtSubchunk = ParseFmtSubchunk(reader);
        var dataSubchunk = ParseDataSubchunk(reader);

        return new AudioData(riffChunk, fmtSubchunk, dataSubchunk);
    }

    public static AudioData Parse(byte[] rawAudioData)
    {
        var memoryStream = new MemoryStream(rawAudioData);
        var reader = new BinaryReader(memoryStream);
        var riffChunk = ParseRiffChunk(reader);
        var fmtSubchunk = ParseFmtSubchunk(reader);
        var dataSubchunk = ParseDataSubchunk(reader);

        return new AudioData(riffChunk, fmtSubchunk, dataSubchunk);
    }

    /// <summary>
    /// Extracts and parses the Riff chunk section of the audio file
    /// </summary>
    /// <returns>A struct containing the Riff chunk data</returns>
    private static RiffChunk ParseRiffChunk(BinaryReader reader)
    {
        var riffChunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
        var chunkSize = reader.ReadInt32();
        var format = Encoding.ASCII.GetString(reader.ReadBytes(4));

        return new RiffChunk(riffChunkId, chunkSize, format);
    }

    /// <summary>
    /// Extracts and parses the Fmt sub chunk
    /// </summary>
    /// <returns>A struct containing the Fmt sub chunk data</returns>
    private static FmtSubchunk ParseFmtSubchunk(BinaryReader reader)
    {
        var subchunk1Id = Encoding.ASCII.GetString(reader.ReadBytes(4));
        var subchunk1Size = reader.ReadInt32();
        var audioFormat = reader.ReadInt16();
        var numChannels = reader.ReadInt16();
        var sampleRate = reader.ReadInt32();
        var byteRate = reader.ReadInt32();
        var blockAlign = reader.ReadInt16();
        var bitsPerSample = reader.ReadInt16();

        return new FmtSubchunk(subchunk1Id, subchunk1Size, audioFormat, numChannels,
            sampleRate, byteRate, blockAlign, bitsPerSample);
    }

    /// <summary>
    /// Extracts and parses the data sub chunk of the audio file
    /// </summary>
    /// <returns>A struct containing the data section of the audio file</returns>
    private static DataSubchunk ParseDataSubchunk(BinaryReader reader)
    {
        var subchunk2Id = Encoding.ASCII.GetString(reader.ReadBytes(4));
        var subchunk2Size = reader.ReadInt32();
        var data = reader.ReadBytes(subchunk2Size);

        return new DataSubchunk(subchunk2Id, subchunk2Size, data);
    }
}
