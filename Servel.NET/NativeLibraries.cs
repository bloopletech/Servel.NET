using Microsoft.Extensions.FileProviders;
using SQLitePCL;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Servel.NET;

public static class NativeLibraries
{
    private static readonly ManifestEmbeddedFileProvider FileProvider = new(Assembly.GetEntryAssembly()!);
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
        LibrariesDirectory!.Delete(true);
    }

    private static void ExtractLibraries()
    {
        LibrariesDirectory = Directory.CreateTempSubdirectory("Servel.NET");

        var directoryContents = FileProvider.GetDirectoryContents("NativeLibraries");

        foreach(var fileInfo in directoryContents)
        {
            var fileName = fileInfo.Name;
            var nativePath = Path.Join(LibrariesDirectory.FullName, fileName);

            using var embeddedStream = fileInfo.CreateReadStream();
            using var fileStream = File.Create(nativePath);
            embeddedStream.CopyTo(fileStream);

            if(OperatingSystem.IsWindows()) Libraries.Add(fileName[..^4], nativePath); //nativedep -> nativedep.dll
            else if(OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                if(fileName.StartsWith("lib")) Libraries.Add(fileName[3..], nativePath); //nativedep.{so|dylib} -> libnativedep.{so|dylib}
                else Libraries.Add(fileName, nativePath); //nativedep.{so|dylib} -> nativedep.{so|dylib}
            }
        }
    }

    private static void ConfigureLibraryResolvers()
    {
        NativeLibrary.SetDllImportResolver(typeof(SQLite3Provider_e_sqlite3).Assembly, DllImportResolver);
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if(Libraries.TryGetValue(libraryName, out var nativePath)) return NativeLibrary.Load(nativePath);
        return IntPtr.Zero;
    }

    private static void LoadLibraries()
    {
        raw.SetProvider(new SQLite3Provider_e_sqlite3());
    }
}
