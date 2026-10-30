using Microsoft.Data.Sqlite;

namespace Servel.NET.Db;

public class Repository<T>(SqliteConnection connection) where T : IModel<T>
{
    private readonly SqliteQueries db = new(connection);

    public void CreateSchema()
    {
        T.CreateSchema();
    }

    public T Create(T record)
    {
        if(record is ITimestampedModel timestampedRecord)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            timestampedRecord.CreatedAt = now;
            timestampedRecord.UpdatedAt = now;
        }

        record.Id = db.Insert(T.Table, record.Values());
        return record;
    }

    public void Update(T record)
    {
        if(record is ITimestampedModel timestampedRecord)
        {
            timestampedRecord.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        db.Update(T.Table, record.Values(), "Id=@Id", [new SqliteParameter("Id", record.Id)]);
    }

    public void Delete(T record)
    {
        db.Delete(T.Table, "Id=@Id", [new SqliteParameter("Id", record.Id)]);
    }

    public T? TryGet(long id)
    {
        return Select(["*"], "Id=@id", [new SqliteParameter("Id", id)]).FirstOrDefault();
    }

    public T Get(long id)
    {
        return TryGet(id) ?? throw new RecordNotFoundException($"No {T.Table} found with Id {id}");
    }

    public T? TryFind(string selection, SqliteParameter[] selectionArgs)
    {
        return Select(selection, selectionArgs).FirstOrDefault();
    }

    public T Find(string selection, SqliteParameter[] selectionArgs)
    {
        return TryFind(selection, selectionArgs) ?? throw new RecordNotFoundException($"No {T.Table} found for query");
    }

    public IList<T> Select(string selection, SqliteParameter[] selectionArgs)
    {
        using var reader = db.Select(T.Table, ["*"], selection, selectionArgs);

        var results = new List<T>();
        while(reader.Read()) results.Add(T.Load(reader));
        return results;
    }

    public long Count()
    {
        return db.Query<long>($"SELECT COUNT(*) FROM {T.Table}", []);
    }
}
