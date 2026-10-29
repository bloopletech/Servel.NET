using Microsoft.Data.Sqlite;

namespace Servel.NET.Db;

public interface IModel<T> where T : IModel<T>
{
    static abstract void CreateSchema();
    static abstract T Load(SqliteDataReader reader);
    SqliteParameter[] Values();
    //void Save();
    //void Delete();
    long? Id { get; set; }
    bool IsNew { get; }
    bool IsExisting { get; }
    //string Table { get; }
    static abstract string Table { get; }
}
