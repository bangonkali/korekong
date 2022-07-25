using Xabe.FFmpeg;

namespace Korekong.Entities;

public class SubtitleInfo
{
    public ISubtitleStream SubtitleStream { get; set; }
    public string SubtitleText { get; set; }
    public string Language { get; set; }

    public long Size { get; set; }
}