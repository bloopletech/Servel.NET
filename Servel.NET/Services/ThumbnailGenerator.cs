using ImageMagick;

namespace Servel.NET.Services;

public class ThumbnailGenerator
{
    public static string[] IMAGE_EXTS = [".jfif", ".jpg", ".jpeg", ".png", ".gif", ".webp"];
    public static string[] VIDEO_EXTS = [".ogg", ".ogm", ".ogv", ".m4v", ".mkv", ".mp4", ".webm", ".avi", ".wmv"];
    public static string[] AUDIO_EXTS = [".aac", ".oga", ".opus", ".m4a", ".mka", ".mp3", ".wav", ".flac", ".wma"];
    public static string[] DOCUMENT_EXTS = [".htm", ".html", ".md", ".pdf", ".txt", ".doc", ".docx"];
    public static string[] COMPRESSED_EXTS = [".bz2", ".gz", ".lz", ".lz4", ".lzma", ".xz", ".7z", ".rar", ".tgz", ".txz", ".rar", ".zip"];

    public byte[]? Thumbnail(string path)
    {
        var extension = Path.GetExtension(path);
        if(VIDEO_EXTS.Contains(extension) || IMAGE_EXTS.Contains(extension)) return ThumbnailImage(path);
        return null;
    }

    private byte[]? ThumbnailImage(string path)
    {
        //TODO: Error handling, manga specific thumbnailing etc
        using var image = new MagickImage(path);
        image.Resize(300, 300);
        image.Quality = 90;
        image.Format = MagickFormat.Jpeg;
        return image.ToByteArray();
    }
}
