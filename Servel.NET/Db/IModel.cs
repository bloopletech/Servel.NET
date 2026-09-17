using Microsoft.Data.Sqlite;

namespace Servel.NET.Db;

public interface IModel<T> where T : IModel<T>
{
    static abstract void CreateSchema();
    static abstract T Load(SqliteDataReader reader);
    void Save();
    void Delete();
    bool IsNew { get; }
    bool IsExisting { get; }
}
