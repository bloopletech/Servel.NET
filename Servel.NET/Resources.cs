using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Reflection;

namespace Servel.NET;

public static class Resources
{
    public static IFileProvider FileProvider { get; private set; }
    public static IFileProvider AssetsFileProvider { get; private set; }

    static Resources()
    {
        FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetEntryAssembly()!);
        AssetsFileProvider = new ManifestEmbeddedFileProvider(Assembly.GetEntryAssembly()!, "Assets");
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
        if(!fileInfo.Exists) throw new FileNotFoundException();

        using var stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
