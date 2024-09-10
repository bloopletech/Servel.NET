#if !DEBUG
using Microsoft.AspNetCore.Http;
using System.Reflection;
#endif

namespace Servel.NET;

public static class StaticResources
{
    public static string GetView(string name)
    {
        return Get(Path.Combine("Views", name));
    }

    public static string Get(string path)
    {
#if DEBUG
        return GetDebug(path);
#else
        return GetRelease(path);
#endif
    }

    private static string GetDebug(string path)
    {
        return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), path));
    }

#if !DEBUG
    private static string GetRelease(string path)
    {
        var resourcePath = path.Replace(Path.DirectorySeparatorChar, '.');
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Servel.NET.{resourcePath}");
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }
#endif
}
