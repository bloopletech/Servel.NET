global using DbValue = System.Collections.Generic.KeyValuePair<string, object?>;
using Microsoft.Data.Sqlite;

namespace Servel.NET.Extensions;

public static class SqliteConnectionExtensions
{
    public static int ExecuteSQL(this SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteNonQuery();
    }

    public static long Insert(
        this SqliteConnection connection,
        string table,
        params (string, object?)[] entries)
    {
        var columnsClause = string.Join(",", entries.Select(e => e.Item1));
        var valuesClause = string.Join(",", entries.Select((e, i) => $"@{i}");

        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = $"INSERT INTO {table} ({columnsClause}) VALUES ({valuesClause})";
        foreach (var (index, entry) in entries.Index())
        {
            insertCommand.Parameters.AddWithValue($"@{index}", entry.Item2);
        }
        insertCommand.ExecuteNonQuery();

        var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT last_insert_rowid();";
        return (long)idCommand.ExecuteScalar()!;
    }

    public static void Update(
        this SqliteConnection connection,
        string table,
        (string, object?) idEntry,
        params List<(string, object?)> entries)
    {
        var setClause = string.Join(", ", entries.Select((e, i) => $"{e.Item1} = @{i}"));

        var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = $"UPDATE {table} SET {setClause} WHERE {idEntry.Item1} = @id";
        foreach (var (index, entry) in entries.Index())
        {
            updateCommand.Parameters.AddWithValue($"@{index}", entry.Item2);
        }
        updateCommand.Parameters.AddWithValue("@id", idEntry.Item2);
        updateCommand.ExecuteNonQuery();
    }
}
