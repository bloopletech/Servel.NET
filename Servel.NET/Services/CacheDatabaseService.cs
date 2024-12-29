using Servel.NET.Models;

namespace Servel.NET.Services;

public class CacheDatabaseService(string databasePath) : DatabaseServiceBase(databasePath)
{
    public override void CreateSchema()
    {
        using var db = Connect();
        //MediaFileMetadata.CreateSchema(db);
        Thumbnail.CreateSchema(db);
    }
}
