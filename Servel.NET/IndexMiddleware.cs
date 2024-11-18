using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

namespace Servel.NET;

// Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/DirectoryBrowserMiddleware.cs
public class IndexMiddleware(
    RequestDelegate next,
    Listing listing,
    DirectoryOptionsResolver directoryOptionsResolver,
    IMemoryCache memoryCache)
{
    private readonly EntryFactory _entryFactory = new(listing, memoryCache);

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (ShouldProcess(httpContext)) await Process(httpContext);
        else await next.Invoke(httpContext);
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

        httpContext.Response.Headers.Vary = HeaderNames.Accept;

        if (httpContext.Request.Headers.Accept.Contains(MediaTypeNames.Application.Json))
        {
            await Render(httpContext);
        }
        else
        {
            await Results.Text(Resources.Get("Views", "index.html"), MediaTypeNames.Text.Html).ExecuteAsync(httpContext);
        }
    }

    private async Task Render(HttpContext httpContext)
    {
        var directoryOptions = directoryOptionsResolver.Resolve(httpContext.Request.PathBase + httpContext.Request.Path);

        var responseJson = RenderResponse(
            httpContext.Request.Path,
            ParseParameters(httpContext, directoryOptions),
            directoryOptions);

        if (responseJson == null)
        {
            await Results.NotFound().ExecuteAsync(httpContext);
            return;
        }

        await Results.Text(responseJson, MediaTypeNames.Application.Json).ExecuteAsync(httpContext);
    }

    private static EntryFactory.ForDirectoryOptions ParseParameters(HttpContext httpContext, DirectoryOptions options)
    {
        var parameters = IndexParameters.Parse(httpContext.Request);
        var defaultParams = options.DefaultParameters;

        return new EntryFactory.ForDirectoryOptions
        {
            Depth = (parameters?.Depth ?? defaultParams.Depth) + 1,
            CountChildren = parameters?.CountChildren ?? defaultParams.CountChildren
        };
    }

    private byte[]? RenderResponse(
        PathString path,
        EntryFactory.ForDirectoryOptions options,
        DirectoryOptions directoryOptions)
    {
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(
                new IndexResponse(_entryFactory.ForDirectory(path, options), directoryOptions.DefaultQuery),
                SerializationSourceGenerationContext.Default.IndexResponse);
        }
        catch(DirectoryNotFoundException)
        {
            return null;
        }
    }
}
