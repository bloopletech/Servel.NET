#if WINDOWS
using System.Runtime.InteropServices;
using SQLitePCL;

namespace Servel.NET.NativeLibraries;

public static partial class NativeLibraries
{
    private static void ExtractLibraries()
    {
        CreateLibrariesDirectory();
        ExtractLibrary("e_sqlite3.dll", "e_sqlite3");
    }

    private static void ConfigureLibraryResolvers()
    {
        NativeLibrary.SetDllImportResolver(typeof(SQLite3Provider_e_sqlite3).Assembly, DllImportResolver);
    }

    private static void LoadLibraries()
    {
        raw.SetProvider(new SQLite3Provider_e_sqlite3());
        //raw.SetProvider(new SQLite3Provider_winsqlite3());
    }
}
#endif
