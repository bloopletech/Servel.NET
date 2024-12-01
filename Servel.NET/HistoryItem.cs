using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET;

public record struct HistoryItem(long? Id, string Path, long LastVisited, int VisitedCount)
{
    public HistoryItem(DbDataReader reader) : this(
        reader.GetInt64("id"),
        reader.GetString("path"),
        reader.GetInt64("last_visited"),
        reader.GetInt32("visited_count"))
    {
    }

    public void Save(SqliteConnection db)
    {
        if(Id != null)
        {
            db.Update(
                "history",
                ("id", Id.Value),
                ("path", Path),
                ("last_visited", LastVisited),
                ("visited_count", VisitedCount));
        }
        else
        {
            Id = db.Insert(
                "history",
                ("path", Path),
                ("last_visited", LastVisited),
                ("visited_count", VisitedCount));
        }
    }

    public static HistoryItem? FindById(SqliteConnection db, long id)
    {
        var command = db.CreateCommand();
        command.CommandText = "SELECT * FROM history WHERE id=@Id";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read()) return new HistoryItem(reader);
        return null;
    }

    public static HistoryItem? FindByPath(SqliteConnection db, string path)
    {
        var command = db.CreateCommand();
        command.CommandText = "SELECT * FROM history WHERE path=@Path";
        command.Parameters.AddWithValue("@Path", path);

        using var reader = command.ExecuteReader();
        if (reader.Read()) return new HistoryItem(reader);
        return null;
    }

    public static HistoryItem FindOrCreateByPath(SqliteConnection db, string path)
    {
        var existing = FindByPath(db, path);
        if (existing.HasValue) return existing.Value;

        var item = new HistoryItem();
        item.Path = path;
        item.Save(db);
        return item;
    }
}
