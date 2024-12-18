using System.Data;
using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;
using Servel.NET.Models;

namespace Servel.NET.Services;

public class DatabaseService(string databasePath)
{
    private readonly string _connectionString = new SqliteConnectionStringBuilder()
    {
        DataSource = databasePath
    }.ToString();

    public void CreateSchema()
    {
        using var db = Connect();
        HistoryItem.CreateSchema(db);
    }

    public SqliteConnection Connect()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Enable write-ahead logging
        connection.Query(@"PRAGMA journal_mode = 'wal'");

        return connection;
    }
}
