namespace Servel.NET.Db;

public class DatabaseService(string databasePath) : DatabaseServiceBase(databasePath)
{
    public override void CreateSchema()
    {
        using var db = Connect();
        HistoryItem.CreateSchema(db);
    }
}
