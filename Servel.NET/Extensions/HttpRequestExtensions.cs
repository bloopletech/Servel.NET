namespace Servel.NET.Extensions;

public static class HttpRequestExtensions
{
    // Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/Helpers.cs
    public static bool IsGetOrHead(this HttpRequest request) =>
        HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method);

    public static bool IsRoot(this HttpRequest request) => request.Path.IsRoot();

    public static PathString FullPath(this HttpRequest request) => request.PathBase + request.Path;
}
