using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET.Models;

public class Thumbnail
{
    public long? Id { get; set; }
    public required string Path { get; set; }
    public byte[]? Data { get; set; }
    public long Mtime { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }

    public const string Table = "Thumbnails";

    public Thumbnail()
    {
    }

    [SetsRequiredMembers]
    public Thumbnail(DbDataReader reader)
    {
        Id = reader.GetInt64("Id");
        Path = reader.GetString("Path");
        Data = reader.GetByteArray("Data");
        Mtime = reader.GetInt64("Mtime");
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
            new("Data", Data),
            new("Mtime", Mtime),
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


    public static Thumbnail? FindById(SqliteConnection db, long id)
    {
        return db.Get(Table, new DbPair("Id", id), reader => new Thumbnail(reader));
    }

    public static Thumbnail? FindByPath(SqliteConnection db, string path)
    {
        return db.Get(
            $"SELECT * FROM {Table} WHERE Path=@Path",
            [new SqliteParameter("@Path", path)],
            reader => new Thumbnail(reader));
    }

    public static void CreateSchema(SqliteConnection db)
    {
        db.Query($"""
        CREATE TABLE IF NOT EXISTS {Table} (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Path TEXT NOT NULL,
            Data BLOB,
            Mtime INTEGER NOT NULL,
            CreatedAt INTEGER NOT NULL,
            UpdatedAt INTEGER NOT NULL
        )
        """);
    }
}
