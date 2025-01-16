using CliWrap;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Servel.NET.Services;

public class ThumbnailGenerator
{
    public static string[] IMAGE_EXTS = [".jfif", ".jpg", ".jpeg", ".png", ".gif", ".webp"];
    public static string[] VIDEO_EXTS = [".ogg", ".ogm", ".ogv", ".m4v", ".mkv", ".mp4", ".webm", ".avi", ".wmv"];
    public static string[] AUDIO_EXTS = [".aac", ".oga", ".opus", ".m4a", ".mka", ".mp3", ".wav", ".flac", ".wma"];
    public static string[] DOCUMENT_EXTS = [".htm", ".html", ".md", ".pdf", ".txt", ".doc", ".docx"];
    public static string[] COMPRESSED_EXTS = [".bz2", ".gz", ".lz", ".lz4", ".lzma", ".xz", ".7z", ".rar", ".tgz", ".txz", ".rar", ".zip"];
    public static int WIDTH = 300;
    public static int HEIGHT = 300;

    public async Task<byte[]?> Thumbnail(string path)
    {
        var extension = Path.GetExtension(path);
        if(VIDEO_EXTS.Contains(extension)) return await ThumbnailVideo(path);
        if(IMAGE_EXTS.Contains(extension)) return await ThumbnailImage(path);
        return null;
    }

    private async Task<byte[]?> ThumbnailImage(string path)
    {
        //TODO: Error handling, manga specific thumbnailing etc
        using var image = await Image.LoadAsync(path);
        return await ResizeImage(image);
    }

    private async Task<byte[]?> ThumbnailVideo(string path)
    {
        using var ffmpegStream = new MemoryStream();

        await Cli
            .Wrap("ffmpeg.exe")
            .WithArguments([
                "-hide_banner",
                "-loglevel",
                "error",
                "-i",
                path,
                "-vf",
                "thumbnail,scale=300:300:force_original_aspect_ratio=increase",
                "-frames:v",
                "1",
                "-c:v",
                "png",
                "-f",
                "image2pipe",
                "-"])
            //.WithWorkingDirectory("work/dir/path")
            .WithStandardOutputPipe(PipeTarget.ToStream(ffmpegStream))
            .ExecuteAsync();

        ffmpegStream.Position = 0;

        using var image = await Image.LoadAsync(ffmpegStream);
        return await ResizeImage(image);
    }

    private async Task<byte[]> ResizeImage(Image image)
    {
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(WIDTH, HEIGHT)
        }));

        using var memoryStream = new MemoryStream();
        await image.SaveAsJpegAsync(memoryStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
        {
            Quality = 90
        });
        return memoryStream.ToArray();
    }
}
