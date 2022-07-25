namespace Korekong.Extensions;

public static class StringPathExtensions
{
    public static string AsPathWithExtension(this string input, string ext)
    {
        var dir = Path.GetDirectoryName(input.Replace("\"", ""));
        var filename = Path.GetFileNameWithoutExtension(input.Replace("\"", ""));
        var path = Path.Combine(dir, filename);
        var output = $"{path}.{ext}";
        return output;
    }

    public static bool AsPathWithExtensionExists(this string input, string ext)
    {
        return File.Exists(input.AsPathWithExtension(ext));
    }
}