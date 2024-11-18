namespace Servel.NET.Extensions;

public static class StringExtensions
{
    public static bool EqualsIgnoreCase(this string a, string b)
    {
        return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
    }

    public static string EnsureTrailingSlash(this string? path)
    {
        if (!string.IsNullOrEmpty(path) && path[^1] == '/') return path;
        return path + '/';
    }
}
