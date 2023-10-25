using System.Reflection;

namespace Servel.NET;

public static class StaticViews
{
    public static string Get(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Servel.NET.Views.{name}");
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }
}
