using Microsoft.Data.Sqlite;

namespace Servel.NET.Db;

public class HistoryItemsRepository(SqliteConnection connection) : Repository<HistoryItem>(connection)
{
    public HistoryItem? FindBySiteAndPath(int siteId, string path)
    {
        return TryFind($"SiteId=@SiteId AND Path=@Path", [new("@SiteId", siteId), new("@Path", path)]);
    }

    public IList<HistoryItem> SelectRecent(int siteId, int limit)
    {
        return Select(
            "SiteId=@SiteId",
            "LastVisited DESC, VisitedCount DESC",
            "@Limit",
            [new("@SiteId", siteId), new("@Limit", limit)]);
    }

    public IList<HistoryItem> SelectPopular(int siteId, int limit)
    {
        return Select(
            "SiteId=@SiteId",
            "VisitedCount DESC, LastVisited DESC",
            "@Limit",
            [new("@SiteId", siteId), new("@Limit", limit)]);
    }
}
