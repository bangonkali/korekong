using System.CommandLine;
using Korekong.Entities;
using Korekong.Extensions;
using Xabe.FFmpeg;

namespace Korekong;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        var test = new Option<bool>(
            name: "--test",
            description: "Skip conversion but read all videos to see sanity of input.");
        test.SetDefaultValue(false);
        test.IsRequired = false;
        test.AddAlias("-t");

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
        rootCommand.AddOption(test);

        rootCommand.SetHandler(async (testOnly, skipConfirm, inputDir) =>
            {
                Console.WriteLine("Starting conversion");

                var results = await Convert(testOnly, skipConfirm, inputDir!);
                foreach (var result in results.ConversionResults)
                {
                    Console.WriteLine($"Converted {result.Arguments} in {result.Duration}");
                }

                Console.WriteLine(results.Message);
            },
            test,
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

    static async Task<KonvertResult> Convert(bool testOnly, bool skipConfirm, DirectoryInfo inputDir)
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
        var aggregateCodecs = new HashSet<string>();
        foreach (var file in files)
        {
            try
            {
                var mediaInfo = await FFmpeg.GetMediaInfo(file);
                var codecs = mediaInfo.GetSubtitleCodecs();
                foreach (var codec in codecs)
                {
                    aggregateCodecs.Add(codec);
                }

                Console.WriteLine($"{file} {string.Join(", ", codecs)}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        Console.WriteLine($"{aggregateCodecs.Count()} subtitle codec/s found:");
        foreach (var codec in aggregateCodecs)
        {
            Console.WriteLine($"{codec}");
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

            Console.WriteLine($"ffmpeg {conversion.Build()}");

            if (!testOnly)
            {
                try
                {
                    var conversionResult = await conversion.Start();
                    results.Add(conversionResult);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return new KonvertResult
        {
            ConversionResults = results,
            Message = "Completed successfully!"
        };
    }
}