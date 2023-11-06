using System.Text;
using CommandLine;

namespace AudioSteganography;


public class Program
{
    public static void Main(string[] args)
    {
        var parsedArgs = Parser.Default.ParseArguments<Options>(args);

        var inputFile = parsedArgs.Value.InputFile;
        var outputFile = parsedArgs.Value.OutputFile;
        var encode = parsedArgs.Value.Encode;
        var decode = parsedArgs.Value.Decode;

        if (decode)
        {
            var encodedAudioData = WavParser.Parse(inputFile!);
            var decodedData = Decoder.Decode(encodedAudioData);

            if (outputFile is not null)
            {
                using var writer = new StreamWriter(outputFile);
                writer.Write(Encoding.UTF8.GetString(decodedData));
                Console.WriteLine($"[INFO] File saved in {Path.Join(Directory.GetCurrentDirectory(), outputFile)}");
            }
            else
            {
                Console.WriteLine(Encoding.UTF8.GetString(decodedData));
            }
            return;
        }

        if (!encode) return;
        var data = parsedArgs.Value.Data ?? Console.In.ReadToEnd();
        var rawAudioData = WavParser.Parse(inputFile!);
        var encodedData = Encoder.Encode(rawAudioData, Encoding.UTF8.GetBytes(data));
        var filePath = WavParser.Parse(encodedData).Save(outputFile!);
        Console.WriteLine($"[INFO] File saved in {filePath}");
    }
}

public class Options
{
    [Option('i', "input", Required = true, HelpText = "Input .wav file")]
    public string? InputFile { get; set; }

    [Option('o', "output", Required = false, HelpText = "Output .wav file")]
    public string? OutputFile { get; set; }

    [Option('d', "data", Required = false, HelpText = "Data to encode. Can be raw data or file path")]
    public string? Data { get; set; }

    [Option('e', "encode", Required = false, Default = true, HelpText = "Encode data")]
    public bool Encode { get; set; }

    [Option('d', "decode", Required = false, Default = false, HelpText = "Decode data")]
    public bool Decode { get; set; }
}
