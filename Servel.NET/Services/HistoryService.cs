using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;
using Servel.NET.Extensions;
using Servel.NET.Models;

namespace Servel.NET.Services;

public class HistoryService(DatabaseService databaseService)
{
    private static readonly TimeSpan MaxSameFileDuration = new(12, 0, 0);

    public void VisitDirectory(int siteId, string path)
    {
        using var db = databaseService.Connect();

        var historyItem = HistoryItem.FindBySiteAndPath(db, siteId, path) ?? new HistoryItem
        {
            SiteId = siteId,
            Path = path
        };
        historyItem.ItemType = HistoryItem.HistoryItemItemType.Directory;
        historyItem.LastVisited = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        historyItem.VisitedCount++;
        historyItem.Save(db);
    }

    public void VisitDirectory(HttpContext httpContext)
    {
        VisitDirectory(httpContext.SiteId(), httpContext.Request.FullPath());
    }

    public static async Task VisitDirectoryBackground(IBackgroundTaskQueue queue, HttpContext httpContext)
    {
        var siteId = httpContext.SiteId();
        var path = httpContext.Request.FullPath();
        await queue.QueueAsync(async (services, cancellationToken) =>
        {
            var historyService = services.GetRequiredService<HistoryService>();
            await Task.Run(() => historyService.VisitDirectory(siteId, path), cancellationToken);
        });
    }

    private void VisitFile(int siteId, string path, bool partial = false)
    {
        var now = DateTimeOffset.Now;
        using var db = databaseService.Connect();

        var historyItem = HistoryItem.FindBySiteAndPath(db, siteId, path) ?? new HistoryItem
        {
            SiteId = siteId,
            Path = path
        };

        var isAVisit = true;
        if(historyItem.IsExisting && partial)
        {
            var lastVisited = DateTimeOffset.FromUnixTimeMilliseconds(historyItem.LastVisited);
            if(now - lastVisited <= MaxSameFileDuration) isAVisit = false;
        }

        historyItem.ItemType = HistoryItem.HistoryItemItemType.File;
        historyItem.LastVisited = now.ToUnixTimeMilliseconds();
        if(isAVisit) historyItem.VisitedCount++;
        historyItem.Save(db);
    }

    public void VisitFile(HttpContext httpContext)
    {
        VisitFile(
            httpContext.SiteId(),
            httpContext.Request.FullPath(),
            !StringValues.IsNullOrEmpty(httpContext.Response.Headers.ContentRange));
    }

    public static void OnPrepareResponse(StaticFileResponseContext staticFileResponseContext)
    {
        var httpContext = staticFileResponseContext.Context;
        var historyService = httpContext.RequestServices.GetService<HistoryService>();
        historyService?.VisitFile(httpContext);
    }

    public IList<HistoryItem> GetRecent(int siteId)
    {
        using var db = databaseService.Connect();
        return HistoryItem.SelectRecent(db, siteId, 10);
    }

    public IList<HistoryItem> GetPopular(int siteId)
    {
        using var db = databaseService.Connect();
        return HistoryItem.SelectPopular(db, siteId, 10);
    }
}
