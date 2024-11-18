namespace Servel.NET.Extensions;

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

    // Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/Helpers.cs
    public static bool EndsInSlash(this PathString path)
    {
        return path.HasValue && path.Value!.EndsWith('/');
    }
}
