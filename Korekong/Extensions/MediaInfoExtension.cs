using Xabe.FFmpeg;
using Xabe.FFmpeg.Streams.SubtitleStream;

namespace Korekong.Extensions;

public static class MediaInfoExtension
{
    public static List<string> GetSubtitleCodecs(this IMediaInfo mediaInfo)
    {
        return mediaInfo.SubtitleStreams.Select(c => $"{c.Codec}").ToList();
    }

    public static async Task<Conversion> ConvertTo_H264_AAC(this IMediaInfo mediaInfo)
    {
        var size = "1920:1080";
        var subtitles = await mediaInfo.SubtitleStreams.GetSubtitles();
        var outputPath = mediaInfo.Path.AsPathWithExtension("mp4");

        var conversion = new Conversion();
        IVideoStream? videoStream = mediaInfo.VideoStreams.FirstOrDefault();
        IAudioStream? audioStream = mediaInfo.AudioStreams.FirstOrDefault();

        if (videoStream is null) throw new Exception("No video stream available");
        if (audioStream is null) throw new Exception("No audio stream available");

        videoStream.SetCodec(VideoCodec.h264);
        videoStream.SetSize(VideoSize.Hd1080);

        audioStream.SetCodec(AudioCodec.aac);

        conversion.AddStream(videoStream);
        conversion.AddStream(audioStream);

        // check if there is hdmv_pgs_subtitle in subtitles (candidate for burn-in)
        var engHdmvPgs = subtitles.Where(c => c.SubtitleStream.Codec == "hdmv_pgs_subtitle")
            .Where(c => c.Language == "eng")
            .MaxBy(c => c.Size);

        // find the subtitle index based on the largest english hdmv_pgs_subtitle & draw as overlay on top of video
        if (engHdmvPgs is not null)
        {
            var all = mediaInfo.SubtitleStreams.ToList();
            for (var i = 0; i < all.Count(); i++)
            {
                if (all[i].Index != engHdmvPgs.SubtitleStream.Index) continue;
                var pad = $"force_original_aspect_ratio=decrease,pad={size}"; // force aspect ratio & size
                var center = $"(ow-iw)/2:(oh-ih)/2,setsar=1"; // center the 0:v
                var scale = $"scale={size}"; // ensure size is correct
                var graph = $"[0:v]{scale}:{pad}:{center}[out0]"; // generate out0
                var overlay = $"[out0][0:s:{i}]overlay"; // overlay subtitle i on top of out0
                conversion.AddParameter($"-lavfi \"{graph};{overlay}\""); // pass to lavfi
                break;
            }
        }

        // get all unique language subrips
        var subrips = subtitles.Where(c => c.SubtitleStream.Codec == "subrip")
            .ToList();

        // candidate for embedding as mov_text
        foreach (var subtitle in subrips)
        {
            var sub = subtitle.SubtitleStream;
            sub.SetCodec(SubtitleCodec.mov_text);
            sub.SetLanguage(subtitle.Language);
            conversion.AddStream(sub);
        }

        conversion.SetPreset(ConversionPreset.VerySlow);
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