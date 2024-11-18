using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Reflection;

namespace Servel.NET;

public static class Resources
{
    public static IFileProvider FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetEntryAssembly()!);
    public static IFileProvider AssetsFileProvider = new ManifestEmbeddedFileProvider(
        Assembly.GetEntryAssembly()!,
        "Assets");

    static Resources()
    {
        ConfigureDebug();
    }

    [Conditional("DEBUG")]
    private static void ConfigureDebug()
    {
        FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
        AssetsFileProvider = new PhysicalFileProvider(Path.Join(Directory.GetCurrentDirectory(), "Assets"));
    }

    public static string Get(params string[] parts)
    {
        var fileInfo = FileProvider.GetFileInfo(Path.Join(parts));
        if (!fileInfo.Exists) throw new FileNotFoundException();

        using var stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
