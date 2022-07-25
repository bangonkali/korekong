using System.CommandLine;
using Korekong.Entities;
using Korekong.Extensions;
using Xabe.FFmpeg;

namespace Korekong;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        var skipConfirmation = new Option<bool>(
            name: "--yes",
            description: "Skip asking for confirmation to start conversion.");
        skipConfirmation.SetDefaultValue(false);
        skipConfirmation.IsRequired = false;
        skipConfirmation.AddAlias("-y");

        var directoryOption = new Option<DirectoryInfo?>(
            name: "--directory",
            description: "The directory to search for '.mkv' files.");
        directoryOption.IsRequired = true;
        directoryOption.AddAlias("-d");

        var rootCommand = new RootCommand("Korekong MKV to MP4 (h264-aac) converter.");

        rootCommand.AddOption(skipConfirmation);
        rootCommand.AddOption(directoryOption);

        rootCommand.SetHandler(async (skipConfirm, inputDir) =>
            {
                Console.WriteLine("Starting conversion");

                var results = await Convert(skipConfirm, inputDir!);
                foreach (var result in results.ConversionResults)
                {
                    Console.WriteLine($"Converted {result.Arguments} in {result.Duration}");
                }

                Console.WriteLine(results.Message);
            },
            skipConfirmation,
            directoryOption);

        try
        {
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return -1;
        }
    }

    static async Task<KonvertResult> Convert(bool skipConfirm, DirectoryInfo inputDir)
    {
        // Find all target files, files ending with .mkv
        var results = new List<IConversionResult>();
        var files = Directory.GetFiles(inputDir.FullName, "*.*", SearchOption.AllDirectories)
            .Where(file => file.ToLower().EndsWith(".mkv"))
            .Where(file => !file.AsPathWithExtensionExists("mp4"))
            .ToList();

        // If no files found, then quit early.
        if (files.Count == 0) return KonvertResult.NoFiles();

        // Preview all the files that were found.
        Console.WriteLine($"{files.Count()} file/s found:");
        foreach (var file in files)
        {
            Console.WriteLine($"{file}");
        }

        // Ask the user if they want to continue or not.
        if (!skipConfirm)
        {
            Console.WriteLine($"Press Y key to continue:");
            var input = Console.ReadKey();
            Console.WriteLine();
            if (input.Key != ConsoleKey.Y)
            {
                return KonvertResult.Quit();
            }
        }

        // Loop through all files and do conversion
        foreach (var file in files)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(file);
            var conversion = await mediaInfo.ConvertTo_H264_AAC();
            var conversionResult = await conversion.Start();
            results.Add(conversionResult);
        }

        return new KonvertResult
        {
            ConversionResults = results,
            Message = "Completed successfully!"
        };
    }
}