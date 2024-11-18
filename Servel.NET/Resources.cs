using Microsoft.Extensions.FileProviders;
#if !DEBUG
using System.Reflection;
#endif

namespace Servel.NET;

public static class Resources
{
    public static IFileProvider FileProvider =
#if DEBUG
        new PhysicalFileProvider(Directory.GetCurrentDirectory());
#else
        new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly());
#endif

    public static IFileProvider AssetsFileProvider =
#if DEBUG
        new PhysicalFileProvider(Path.Join(Directory.GetCurrentDirectory(), "Assets"));
#else
        new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "Assets");
#endif

    public static string Get(params string[] parts)
    {
        var fileInfo = FileProvider.GetFileInfo(Path.Join(parts));
        if (!fileInfo.Exists) throw new FileNotFoundException();

        using var stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
