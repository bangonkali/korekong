using Xabe.FFmpeg;

namespace Korekong.Entities;

public class KonvertResult
{
    public List<IConversionResult> ConversionResults { get; set; } = new();

    public string Message { get; set; } = "";

    public static KonvertResult Quit()
    {
        return new KonvertResult() { Message = "User quit!" };
    }
    
    public static KonvertResult NoFiles()
    {
        return new KonvertResult() { Message = "No files found." };
    }
}