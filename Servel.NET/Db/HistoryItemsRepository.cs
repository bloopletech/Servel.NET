using Microsoft.Data.Sqlite;

namespace Servel.NET.Db;

public class HistoryItemsRepository(SqliteConnection connection) : Repository<HistoryItem>(connection)
{
    public HistoryItem? FindBySiteAndPath(int siteId, string path)
    {
        return TryFind($"SiteId=@SiteId AND Path=@Path", [new("@SiteId", siteId), new("@Path", path)]);
    }

    public static IList<HistoryItem> SelectRecent(SqliteConnection db, int siteId, int limit)
    {
        return db.Select(
            $"SELECT * FROM {Table} WHERE SiteId=@SiteId ORDER BY LastVisited DESC, VisitedCount DESC LIMIT @Limit",
            [new SqliteParameter("@SiteId", siteId), new SqliteParameter("@Limit", limit)],
            reader => new HistoryItem(reader));
    }

    public static IList<HistoryItem> SelectPopular(SqliteConnection db, int siteId, int limit)
    {
        return db.Select(
            $"SELECT * FROM {Table} WHERE SiteId=@SiteId ORDER BY VisitedCount DESC, LastVisited DESC LIMIT @Limit",
            [new SqliteParameter("@SiteId", siteId), new SqliteParameter("@Limit", limit)],
            reader => new HistoryItem(reader));
    }
}
