using Korekong.Entities;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Streams.SubtitleStream;

namespace Korekong.Extensions;

public static class SubtitleStreamExtensions
{
    public static string GetSubtitleRawPath(this ISubtitleStream subtitleStream)
    {
        var dir = Path.GetDirectoryName(subtitleStream.Path.Replace("\"", ""));
        var filename = Path.GetFileNameWithoutExtension(subtitleStream.Path.Replace("\"", ""));
        var path = Path.Combine(dir, filename);
        var lang = subtitleStream.Language.ToLower();
        var extension = subtitleStream.Codec == "subrip" ? "srt" : "sup";
        var output = $"{path}.{lang}{subtitleStream.Index}.{extension}";
        return output;
    }

    public static async Task<SubtitleInfo> GetSubtitleInfo(this ISubtitleStream subtitleStream)
    {
        var path = subtitleStream.GetSubtitleRawPath();

        if (File.Exists(path))
            File.Delete(path);

        var extension = subtitleStream.Codec == "subrip" ? Format.srt : Format.sup;

        try
        {
            var conversion = new Conversion();
            conversion.AddStream(subtitleStream);
            subtitleStream.SetCodec(SubtitleCodec.copy);
            conversion.SetOutputFormat(extension);
            conversion.SetOutput(path);
            await conversion.Start();

            var length = new FileInfo(path).Length;

            // only read if the file is text (srt)
            var text = subtitleStream.Codec == "subrip" ? await File.ReadAllTextAsync(path) : "";

            var subtitleInfo = new SubtitleInfo()
            {
                Language = subtitleStream.Language.ToLower(),
                SubtitleStream = subtitleStream,
                SubtitleText = text,
                Size = length,
            };

            // clean scratch files before ending
            if (File.Exists(path))
                File.Delete(path);

            return subtitleInfo;
        }
        finally
        {
            // clean scratch files before ending
            if (File.Exists(path))
                File.Delete(path);
        }
    }


    public static async Task<List<SubtitleInfo>> GetSubtitles(this IEnumerable<ISubtitleStream> subtitleStreams)
    {
        List<SubtitleInfo> subtitles = new List<SubtitleInfo>();

        foreach (var subtitleStream in subtitleStreams)
        {
            if (string.IsNullOrWhiteSpace(subtitleStream.Language)) continue;

            var current = await subtitleStream.GetSubtitleInfo();

            var old = subtitles.FirstOrDefault(s => s.Language == current.Language);
            if (old != null && old.Size <= current.Size)
            {
                // subtitle exists and is smaller than the current one we're working with
                subtitles.Remove(old);
                subtitles.Add(current);
            }
            else if (old == null)
            {
                // subtitle does not exist yet
                subtitles.Add(current);
            }
        }

        return subtitles;
    }
}