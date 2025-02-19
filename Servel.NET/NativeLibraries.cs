using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Servel.NET;

#pragma warning disable IDE0051 // Remove unused private members

public static class NativeLibraries
{
    private static readonly ManifestEmbeddedFileProvider FileProvider = new(
        Assembly.GetEntryAssembly()!,
        "NativeLibraries");
    private static DirectoryInfo? LibrariesDirectory;
    private static readonly Dictionary<string, string> Libraries = [];

    private static void ExtractLibrary(string fileName, string libraryName)
    {
        LibrariesDirectory ??= Directory.CreateTempSubdirectory("Servel.NET");

        var libraryPath = Path.Join(LibrariesDirectory.FullName, fileName);

        using var embeddedStream = FileProvider.GetFileInfo(fileName).CreateReadStream();
        using var fileStream = File.Create(libraryPath);
        embeddedStream.CopyTo(fileStream);

        Libraries.Add(libraryName, libraryPath);
    }

    private static void SetLibraryResolver(Assembly assembly)
    {
        NativeLibrary.SetDllImportResolver(assembly, static (libraryName, _, _) =>
        {
            return Libraries.TryGetValue(libraryName, out var libraryPath) ? NativeLibrary.Load(libraryPath) : IntPtr.Zero;
        });
    }

    public static void OnShutdown()
    {
        LibrariesDirectory?.Delete(true);
    }

#if WIN
    public static void Init()
    {
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_winsqlite3());
    }
#elif LINUX
    public static void Init()
    {
        ExtractLibrary("libe_sqlite3.so", "e_sqlite3.so");
        SetLibraryResolver(typeof(SQLitePCL.SQLite3Provider_e_sqlite3).Assembly);
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
    }
#elif OSX
    public static void Init()
    {
        ExtractLibrary("libe_sqlite3.dylib", "e_sqlite3.dylib");
        SetLibraryResolver(typeof(SQLitePCL.SQLite3Provider_e_sqlite3).Assembly);
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
    }
#endif
}
