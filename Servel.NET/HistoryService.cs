namespace Servel.NET;

public class HistoryService(DatabaseService databaseService)
{
    public static void LoadSchema(DatabaseService db)
    {
        db.ExecuteSQL("""
        CREATE TABLE IF NOT EXISTS history (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            path TEXT,
            last_visited INTEGER DEFAULT 0,
            visited_count INTEGER DEFAULT 0
        )
        """);
    }

    public void Visit(string pathBase, string path)
    {
        using var db = databaseService.CreateConnection();

        var historyItem = HistoryItem.FindOrCreateByPath(db, path);
        historyItem.LastVisited = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        historyItem.VisitedCount++;
        historyItem.Save(db);
    }
}
