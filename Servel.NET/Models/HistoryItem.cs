using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET.Models;

public class HistoryItem
{
    public enum HistoryItemItemType
    {
        Directory,
        File
    };

    public long? Id { get; set; }
    public int SiteId { get; set; }
    public required string Path { get; set; }
    public HistoryItemItemType ItemType { get; set; }
    public long LastVisited { get; set; }
    public int VisitedCount { get; set; }

    public const string Table = "HistoryItems";

    public HistoryItem()
    {
    }

    [SetsRequiredMembers]
    public HistoryItem(DbDataReader reader)
    {
        Id = reader.GetInt64("Id");
        SiteId = reader.GetInt32("SiteId");
        Path = reader.GetString("Path");
        ItemType = (HistoryItemItemType)reader.GetInt32("ItemType");
        LastVisited = reader.GetInt64("LastVisited");
        VisitedCount = reader.GetInt32("VisitedCount");
    }

    public void Save(SqliteConnection db)
    {
        var entries = new DbPair[] {
            new("SiteId", SiteId),
            new("Path", Path),
            new("ItemType", (int)ItemType),
            new("LastVisited", LastVisited),
            new("VisitedCount", VisitedCount)
        };

        if (Id != null) db.Update(Table, new DbPair("Id", Id.Value), entries);
        else Id = db.Insert(Table, entries);
    }

    public void Delete(SqliteConnection db)
    {
        if (Id != null) db.Delete(Table, new DbPair("Id", Id.Value));
    }

    public bool IsNew => Id == null;
    public bool IsExisting => !IsNew;

    public HistoryEntry ToEntry()
    {
        return new HistoryEntry(Path, LastVisited, VisitedCount);
    }

    public static HistoryItem? FindById(SqliteConnection db, long id)
    {
        return db.Get(Table, new DbPair("Id", id), reader => new HistoryItem(reader));
    }

    public static HistoryItem? FindBySiteAndPath(SqliteConnection db, int siteId, string path)
    {
        return db.Get(
            $"SELECT * FROM {Table} WHERE SiteId=@SiteId AND Path=@Path",
            [new SqliteParameter("@SiteId", siteId), new SqliteParameter("@Path", path)],
            reader => new HistoryItem(reader));
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

    public static void CreateSchema(SqliteConnection db)
    {
        db.Query($"""
        CREATE TABLE IF NOT EXISTS {Table} (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SiteId INTEGER,
            Path TEXT,
            ItemType INTEGER,
            LastVisited INTEGER DEFAULT 0,
            VisitedCount INTEGER DEFAULT 0
        )
        """);
    }
}
