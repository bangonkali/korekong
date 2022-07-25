using Xabe.FFmpeg;
using Xabe.FFmpeg.Streams.SubtitleStream;

namespace Korekong.Extensions;

public static class MediaInfoExtension
{
    public static async Task<Conversion> ConvertTo_H264_AAC(this IMediaInfo mediaInfo)
    {
        var subtitles = await mediaInfo.SubtitleStreams.GetSubtitles();
        var outputPath = mediaInfo.Path.AsPathWithExtension("mp4");

        var conversion = new Conversion();
        IStream? videoStream = mediaInfo.VideoStreams.FirstOrDefault()
            ?.SetCodec(VideoCodec.h264);
        IStream? audioStream = mediaInfo.AudioStreams.FirstOrDefault()
            ?.SetCodec(AudioCodec.aac);

        if (videoStream is null) throw new Exception("No video stream available");
        if (audioStream is null) throw new Exception("No audio stream available");

        conversion.AddStream(videoStream);
        conversion.AddStream(audioStream);

        foreach (var subtitle in subtitles)
        {
            var sub = subtitle.SubtitleStream;
            sub.SetCodec(SubtitleCodec.copy);
            sub.SetLanguage(subtitle.Language);
            conversion.AddStream(sub);
        }

        conversion.SetOutputFormat(Format.mp4);
        conversion.SetOutput(outputPath);

        var lastReported = -1;

        conversion.OnProgress += (sender, args) =>
        {
            var percent = (int)(Math.Round(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds, 2) * 100);
            if (percent % 10 == 0 && percent != lastReported)
            {
                lastReported = percent;
                Console.WriteLine($"[{args.Duration} / {args.TotalLength}] {percent}%");
            }
        };

        return conversion;
    }
}