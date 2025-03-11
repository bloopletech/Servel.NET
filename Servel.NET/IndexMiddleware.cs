using Microsoft.Net.Http.Headers;
using Servel.NET.Extensions;
using Servel.NET.Services;
using System.Net.Mime;
using System.Text.Json;

namespace Servel.NET;

// Derived from https://github.com/dotnet/aspnetcore/blob/5a4c82ec57fadddef9ce841d608de5c7c8c74446/src/Middleware/StaticFiles/src/DirectoryBrowserMiddleware.cs
public class IndexMiddleware(
    RequestDelegate next,
    Listing listing,
    DirectoryOptionsResolver directoryOptionsResolver,
    IBackgroundTaskQueue queue,
    HistoryService? historyService = null,
    CacheDatabaseService? cacheDatabaseService = null)
{
    private readonly EntryFactory _entryFactory = new(listing);

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if(ShouldProcess(httpContext)) await Process(httpContext);
        else await next.Invoke(httpContext);
    }

    private static bool ShouldProcess(HttpContext httpContext) => httpContext.Request.IsGetOrHead();

    private async Task Process(HttpContext httpContext)
    {
        // If the path matches a directory but does not end in a slash, redirect to add the slash.
        // This prevents relative links from breaking.
        if(!httpContext.Request.Path.EndsInSlash())
        {
            httpContext.RedirectToPathWithSlash();
            return;
        }

        httpContext.Response.Headers.Vary = HeaderNames.Accept;

        if(httpContext.Request.Headers.Accept.Contains(MediaTypeNames.Application.Json))
        {
            try
            {
                await Render(httpContext);
            }
            catch (DirectoryNotFoundException)
            {
                await Results.NotFound().ExecuteAsync(httpContext);
            }
        }
        else
        {
            await Results.Text(Resources.Get("Views", "index.html"), MediaTypeNames.Text.Html).ExecuteAsync(httpContext);
        }
    }

    private async Task Render(HttpContext httpContext)
    {
        var directoryOptions = directoryOptionsResolver.Resolve(httpContext.Request.FullPath());

        var directoryEntry = _entryFactory.ForDirectory(
            httpContext.Request.Path,
            ParseParameters(httpContext, directoryOptions),
            out var physicalDirectoryInfo);

        if(cacheDatabaseService != null) await ThumbnailService.ThumbnailDirectoryBackground(queue, physicalDirectoryInfo);

        var recentEntries = historyService?.GetRecent(httpContext.SiteId()).Select(hi => hi.ToEntry()).ToList();
        var popularEntries = historyService?.GetPopular(httpContext.SiteId()).Select(hi => hi.ToEntry()).ToList();

        if(historyService != null) await HistoryService.VisitDirectoryBackground(queue, httpContext);
        //historyService?.VisitDirectory(httpContext);

        var indexConfiguration = new ListingConfiguration(cacheDatabaseService != null);

        var indexResponse = new IndexResponse(
            directoryEntry,
            directoryOptions.DefaultQuery,
            indexConfiguration,
            recentEntries,
            popularEntries);

        var responseJson = JsonSerializer.SerializeToUtf8Bytes(
            indexResponse,
            SerializationSourceGenerationContext.Default.IndexResponse);

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
}
