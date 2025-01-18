using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Servel.NET.NativeLibraries;

public static partial class NativeLibraries
{
    private static readonly ManifestEmbeddedFileProvider FileProvider = new(Assembly.GetExecutingAssembly());
    private static DirectoryInfo? LibrariesDirectory;
    private static readonly Dictionary<string, string> Libraries = [];

    public static void Init()
    {
        ExtractLibraries();
        ConfigureLibraryResolvers();
        LoadLibraries();
    }

    public static void OnShutdown()
    {
        LibrariesDirectory?.Delete(true);
    }

    private static void CreateLibrariesDirectory()
    {
        LibrariesDirectory = Directory.CreateTempSubdirectory("Servel.NET");
    }

    private static void ExtractLibrary(string fileName, string libraryName)
    {
        var fileInfo = FileProvider.GetFileInfo($"NativeLibraries/{fileName}");

        var nativePath = Path.Join(LibrariesDirectory!.FullName, fileName);

        using var embeddedStream = fileInfo.CreateReadStream();
        using var fileStream = File.Create(nativePath);
        embeddedStream.CopyTo(fileStream);

        Libraries.Add(libraryName, nativePath);
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if(Libraries.TryGetValue(libraryName, out var nativePath)) return NativeLibrary.Load(nativePath);
        return IntPtr.Zero;
    }
}
