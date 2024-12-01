using System.Data;
using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET;

public class DatabaseService(string databasePath)
{
    public static void LoadSchema(DatabaseService db)
    {
        HistoryService.LoadSchema(db);
    }

    public SqliteConnection CreateConnection()
    {
        var connectionString = new SqliteConnectionStringBuilder()
        {
            DataSource = databasePath
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        // Enable write-ahead logging
        connection.ExecuteSQL(@"PRAGMA journal_mode = 'wal'");

        return connection;
    }

    public int ExecuteSQL(string sql)
    {
        using var db = CreateConnection();
        return db.ExecuteSQL(sql);
    }

    public bool HasColumn(string tableName, string columnName)
    {
        using var db = CreateConnection();

        var command = db.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName})";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (reader.GetString("name") == columnName) return true;
        }

        return false;
    }
}
