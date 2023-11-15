using System.Reflection;

namespace Servel.NET;

public static class StaticViews
{
    public static string Get(string name)
    {
#if DEBUG
        return GetDebug(name);
#else
        return GetRelease(name);
#endif
    }

    private static string GetDebug(string name)
    {
        return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Views", name));
    }

    private static string GetRelease(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Servel.NET.Views.{name}");
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }
}
