using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET.Models;

public class MediaFileMetadata
{
    public long? Id { get; set; }
    public required string Path { get; set; }
    public int? DurationMs { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }

    public const string Table = "MediaFileMetadatas";

    public MediaFileMetadata()
    {
    }

    [SetsRequiredMembers]
    public MediaFileMetadata(DbDataReader reader)
    {
        Id = reader.GetInt64("Id");
        Path = reader.GetString("Path");
        DurationMs = reader.GetInt32("DurationMs");
        CreatedAt = reader.GetInt64("CreatedAt");
        UpdatedAt = reader.GetInt64("UpdatedAt");
    }

    public void Save(SqliteConnection db)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if(Id == null) CreatedAt = now;
        UpdatedAt = now;

        var entries = new DbPair[] {
            new("Path", Path),
            new("DurationMs", DurationMs),
            new("CreatedAt", CreatedAt),
            new("UpdatedAt", UpdatedAt)
        };

        if(Id != null) db.Update(Table, new DbPair("Id", Id.Value), [..entries]);
        else Id = db.Insert(Table, [..entries]);
    }

    public void Delete(SqliteConnection db)
    {
        if(Id != null) db.Delete(Table, new DbPair("Id", Id.Value));
    }

    public bool IsNew => Id == null;
    public bool IsExisting => !IsNew;


    public static MediaFileMetadata? FindById(SqliteConnection db, long id)
    {
        return db.Get(Table, new DbPair("Id", id), reader => new MediaFileMetadata(reader));
    }

    public static MediaFileMetadata? FindByPath(SqliteConnection db, int siteId, string path)
    {
        return db.Get(
            $"SELECT * FROM {Table} WHERE Path=@Path",
            [new SqliteParameter("@Path", path)],
            reader => new MediaFileMetadata(reader));
    }

    public static void CreateSchema(SqliteConnection db)
    {
        db.Query($"""
        CREATE TABLE IF NOT EXISTS {Table} (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Path TEXT NOT NULL,
            DurationMs INTEGER,
            CreatedAt INTEGER NOT NULL,
            UpdatedAt INTEGER NOT NULL
        )
        """);
    }
}
