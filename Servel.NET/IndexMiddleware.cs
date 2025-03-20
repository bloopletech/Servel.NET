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
    CacheDatabaseService? cacheDatabaseService = null) : MiddlewareBase(next)
{
    private readonly EntryFactory _entryFactory = new(listing);

    public override bool ShouldRun() => Request.IsGetOrHead();

    public override IResult? Before()
    {
        // If the path matches a directory but does not end in a slash, redirect to add the slash.
        // This prevents relative links from breaking.
        if(!Request.Path.EndsInSlash()) return HttpContext.RedirectToPathWithSlash();
        return null;
    }

    public override async Task<IResult> RunAsync()
    {
        Response.Headers.Vary = HeaderNames.Accept;

        if(Request.Headers.Accept.Contains(MediaTypeNames.Application.Json))
        {
            try
            {
                return await Render();
            }
            catch (DirectoryNotFoundException)
            {
                return Results.NotFound();
            }
        }

        return Results.Text(Resources.Get("Views", "index.html"), MediaTypeNames.Text.Html);
    }

    private async Task<IResult> Render()
    {
        var directoryOptions = directoryOptionsResolver.Resolve(Request.FullPath());

        var directoryEntry = _entryFactory.ForDirectory(
            Request.Path,
            ParseParameters(directoryOptions),
            out var physicalDirectoryInfo);

        if(cacheDatabaseService != null) await ThumbnailService.ThumbnailDirectoryBackground(queue, physicalDirectoryInfo);

        var recentEntries = historyService?.GetRecent(HttpContext.SiteId()).Select(hi => hi.ToEntry()).ToList();
        var popularEntries = historyService?.GetPopular(HttpContext.SiteId()).Select(hi => hi.ToEntry()).ToList();

        if(historyService != null) await HistoryService.VisitDirectoryBackground(queue, HttpContext);
        //historyService?.VisitDirectory(HttpContext);

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

        return Results.Text(responseJson, MediaTypeNames.Application.Json);
    }

    private EntryFactory.ForDirectoryOptions ParseParameters(DirectoryOptions options)
    {
        var parameters = IndexParameters.Parse(Request);
        var defaultParams = options.DefaultParameters;

        return new EntryFactory.ForDirectoryOptions
        {
            Depth = (parameters?.Depth ?? defaultParams.Depth) + 1,
            CountChildren = parameters?.CountChildren ?? defaultParams.CountChildren
        };
    }
}
