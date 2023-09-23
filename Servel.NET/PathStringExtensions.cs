namespace Servel.NET;

public static class PathStringExtensions
{
    public static bool IsRoot(this PathString pathString)
    {
        return pathString == "/";
    }

    public static PathString Combine(this PathString self, string other)
    {
        return new PathString(EnsureTrailingSlash(self.Value) + EnsureTrailingSlash(other));
    }

    private static string EnsureTrailingSlash(string? path)
    {
        if (!string.IsNullOrEmpty(path) && path[^1] == '/') return path;
        return path + '/';
    }
}
