using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

namespace Servel.NET;

// Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/DirectoryBrowserMiddleware.cs
public class IndexMiddleware(RequestDelegate next, Listing listing, IMemoryCache memoryCache)
{
    private readonly RequestDelegate _next = next;
    private readonly EntryFactory _entryFactory = new EntryFactory(listing, memoryCache);

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (ShouldProcess(httpContext)) await Process(httpContext);
        else await _next.Invoke(httpContext);
    }

    private static bool ShouldProcess(HttpContext httpContext) => FileHelpers.IsGetOrHeadMethod(httpContext.Request.Method);

    private async Task Process(HttpContext httpContext)
    {
        // If the path matches a directory but does not end in a slash, redirect to add the slash.
        // This prevents relative links from breaking.
        if (!FileHelpers.PathEndsInSlash(httpContext.Request.Path))
        {
            FileHelpers.RedirectToPathWithSlash(httpContext);
            return;
        }

        var directoryEntryJson = RenderDirectoryEntry(httpContext.Request.Path, ParseParameters(httpContext));

        if(directoryEntryJson == null)
        {
            await Results.NotFound().ExecuteAsync(httpContext);
            return;
        }

        httpContext.Response.Headers.Vary = HeaderNames.Accept;

        if (httpContext.Request.Headers.Accept.Contains(MediaTypeNames.Application.Json))
        {
            await Results.Text(directoryEntryJson, MediaTypeNames.Application.Json).ExecuteAsync(httpContext);
            return;
        }

        await Results.Text(StaticViews.Get("index.html"), MediaTypeNames.Text.Html).ExecuteAsync(httpContext);
    }

    private static EntryFactory.ForDirectoryOptions ParseParameters(HttpContext httpContext)
    {
        var depthStr = httpContext.Request.Query["depth"];
        _ = uint.TryParse(depthStr.ToString(), out var depth);

        var countChildrenStr = httpContext.Request.Query["countChildren"];
        _ = bool.TryParse(countChildrenStr, out var countChildren);

        return new EntryFactory.ForDirectoryOptions
        {
            Depth = depth + 1,
            CountChildren = countChildren
        };
    }

    private byte[]? RenderDirectoryEntry(PathString path, EntryFactory.ForDirectoryOptions options)
    {
        try
        {
            var directoryEntry = _entryFactory.ForDirectory(path, options);

            return JsonSerializer.SerializeToUtf8Bytes(
                directoryEntry,
                SerializationSourceGenerationContext.Default.DirectoryEntry);
        }
        catch(DirectoryNotFoundException)
        {
            return null;
        }
    }
}
