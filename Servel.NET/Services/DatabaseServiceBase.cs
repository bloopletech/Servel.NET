using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET.Services
{
    public abstract class DatabaseServiceBase(string databasePath)
    {
        private readonly string _connectionString = new SqliteConnectionStringBuilder()
        {
            DataSource = databasePath
        }.ToString();

        public SqliteConnection Connect()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Enable write-ahead logging
            connection.Query(@"PRAGMA journal_mode = 'wal'");

            return connection;
        }

        public abstract void CreateSchema();
    }
}