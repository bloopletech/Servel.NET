#if MACOS
namespace Servel.NET.NativeLibraries;

public static partial class NativeLibraries
{
    private static void ExtractLibraries()
    {
        CreateLibrariesDirectory();
        ExtractLibrary("libe_sqlite3.dylib", "e_sqlite3.dylib");
    }

    private static void ConfigureLibraryResolvers()
    {
        NativeLibrary.SetDllImportResolver(typeof(SQLite3Provider_e_sqlite3).Assembly, DllImportResolver);
    }

    private static void LoadLibraries()
    {
        raw.SetProvider(new SQLite3Provider_e_sqlite3());
    }
}
#endif
