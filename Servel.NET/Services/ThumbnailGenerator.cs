using CliWrap;
using Servel.NET.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Servel.NET.Services;

public class ThumbnailGenerator
{
    public static readonly string[] IMAGE_EXTS = [".jfif", ".jpg", ".jpeg", ".png", ".gif", ".webp"];
    public static readonly string[] VIDEO_EXTS = [".ogg", ".ogm", ".ogv", ".m4v", ".mkv", ".mp4", ".webm", ".avi", ".wmv"];
    public static readonly string[] AUDIO_EXTS = [".aac", ".oga", ".opus", ".m4a", ".mka", ".mp3", ".wav", ".flac", ".wma"];
    public static readonly string[] DOCUMENT_EXTS = [".htm", ".html", ".md", ".pdf", ".txt", ".doc", ".docx"];
    public static readonly string[] COMPRESSED_EXTS = [".bz2", ".gz", ".lz", ".lz4", ".lzma", ".xz", ".7z", ".rar", ".tgz", ".txz", ".rar", ".zip"];
    public const int WIDTH = 300;
    public const int HEIGHT = 300;
    private static readonly ILogger<ThumbnailGenerator> logger = Logging.Create<ThumbnailGenerator>();

    public async Task<byte[]?> Thumbnail(string path)
    {
        var extension = Path.GetExtension(path);
        if(VIDEO_EXTS.Contains(extension)) return await ThumbnailVideo(path);
        if(IMAGE_EXTS.Contains(extension)) return await ThumbnailImage(path);
        return null;
    }

    private static async Task<byte[]?> ThumbnailImage(string path)
    {
        using var measurer = logger.BeginMeasureScope("ThumbnailImage {Path}", path);
        //TODO: Error handling, manga specific thumbnailing etc

        //using var image = await logger.MeasureAsync("Load Image", async () => await Image.LoadAsync(path));
        using var image = await logger.Measure("Load Image", () => Image.LoadAsync(path));
        return await ResizeImage(image);
    }

    private static async Task<byte[]?> ThumbnailVideo(string path)
    {
        using var measurer = logger.BeginMeasureScope("ThumbnailVideo {Path}", path);
        using var ffmpegStream = new MemoryStream();

        using(measurer.Measure("ffmpeg"))
        {
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
        }

        ffmpegStream.Position = 0;

        using var image = await logger.Measure("Load Image", () => Image.LoadAsync(ffmpegStream));
        return await ResizeImage(image);
    }

    private static async Task<byte[]> ResizeImage(Image image)
    {
        using var measurer = logger.BeginMeasureScope("ResizeImage");

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
