using System.CommandLine;
using Korekong.Extensions;
using Xabe.FFmpeg;

namespace Korekong;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        var directoryOption = new Option<DirectoryInfo?>(
            name: "--directory",
            description: "The directory to search for '.mkv' files.");
        directoryOption.IsRequired = true;
        directoryOption.AddAlias("-d");

        var rootCommand = new RootCommand("Korekong MKV to MP4 (h264-aac) converter.");
        rootCommand.AddOption(directoryOption);

        rootCommand.SetHandler(async (inputDir) =>
            {
                Console.WriteLine("Starting conversion");

                var results = await Convert(inputDir!);

                foreach (var result in results)
                {
                    Console.WriteLine($"Converted {result.Arguments} in {result.Duration}");
                }

                Console.WriteLine("Completed conversion");
            },
            directoryOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task<List<IConversionResult>> Convert(DirectoryInfo inputDir)
    {
        var results = new List<IConversionResult>();
        var files = Directory.GetFiles(inputDir.FullName, "*.*", SearchOption.AllDirectories)
            .Where(file => file.ToLower().EndsWith(".mkv"))
            .Where(file => !file.AsPathWithExtensionExists("mp4"))
            .ToList();

        foreach (var file in files)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(file);
            var conversion = await mediaInfo.ConvertTo_H264_AAC();
            var conversionResult = await conversion.Start();
            results.Add(conversionResult);
        }

        return results;
    }
}