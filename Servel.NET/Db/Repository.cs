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
        if(record is ITimestampedModel<T> timestampedModel)
        {

        }
        record.Id = db.Insert(T.Table, record.Values());
        return record;
    }

    public void Update(T record)
    {
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

    public IList<T> Select(string[] columns, string selection, SqliteParameter[] selectionArgs)
    {
        using var reader = db.Select(T.Table, columns, selection, selectionArgs);

        var results = new List<T>();
        while(reader.Read()) results.Add(T.Load(reader));
        return results;
    }

    public long Count()
    {
        return db.Query<long>($"SELECT COUNT(*) FROM {T.Table}", []);
    }
}
