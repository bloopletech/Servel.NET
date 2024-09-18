namespace Servel.NET;

public static class PathStringExtensions
{
    public static bool IsRoot(this PathString pathString)
    {
        return pathString == "/";
    }

    public static PathString Combine(this PathString self, string other)
    {
        return new PathString(self.Value.EnsureTrailingSlash() + other.EnsureTrailingSlash());
    }
}
