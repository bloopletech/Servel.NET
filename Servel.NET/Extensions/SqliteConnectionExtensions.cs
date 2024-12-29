using System.Data;
using Microsoft.Data.Sqlite;

namespace Servel.NET.Extensions;

public readonly record struct DbPair(string Name, object? Value);

public static class SqliteConnectionExtensions
{
    public static SqliteCommand CreateCommand(this SqliteConnection connection, string sql)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        return command;
    }

    public static int Query(this SqliteConnection connection, string sql)
    {
        return connection.Query(sql, []);
    }

    public static int Query(this SqliteConnection connection, string sql, SqliteParameter[] entries)
    {
        using var command = connection.CreateCommand(sql);
        command.Parameters.AddRange(entries);

        return command.ExecuteNonQuery();
    }

    public static T GetRequired<T>(
        this SqliteConnection connection,
        string table,
        DbPair idEntry,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Get(table, idEntry, ["*"], builder) ?? throw new InvalidOperationException();
    }

    public static T GetRequired<T>(
        this SqliteConnection connection,
        string table,
        DbPair idEntry,
        string[] columns,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Get(table, idEntry, columns, builder) ?? throw new InvalidOperationException();
    }

    public static T? Get<T>(
        this SqliteConnection connection,
        string table,
        DbPair idEntry,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Get(table, idEntry, ["*"], builder);
    }

    public static T? Get<T>(
        this SqliteConnection connection,
        string table,
        DbPair idEntry,
        string[] columns,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Select(table, idEntry, columns, builder).FirstOrDefault();
    }

    public static SqliteDataReader Select(
        this SqliteConnection connection,
        string table,
        DbPair idEntry)
    {
        return connection.Select(table, idEntry, ["*"]);
    }

    public static SqliteDataReader Select(
        this SqliteConnection connection,
        string table,
        DbPair idEntry,
        string[] columns)
    {
        var columnsClause = string.Join(",", columns);

        using var command = connection.CreateCommand($"SELECT {columnsClause} FROM {table} WHERE {idEntry.Name} = @id");
        command.Parameters.AddWithValue("@id", idEntry.Value);

        return command.ExecuteReader();
    }

    public static IList<T> Select<T>(
        this SqliteConnection connection,
        string table,
        DbPair idEntry,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Select(table, idEntry, ["*"], builder);
    }

    public static IList<T> Select<T>(
        this SqliteConnection connection,
        string table,
        DbPair idEntry,
        string[] columns,
        Func<SqliteDataReader, T> builder)
    {
        var columnsClause = string.Join(",", columns);

        using var command = connection.CreateCommand($"SELECT {columnsClause} FROM {table} WHERE {idEntry.Name} = @id");
        command.Parameters.AddWithValue("@id", idEntry.Value);

        using var reader = command.ExecuteReader();

        var results = new List<T>();
        while(reader.Read()) results.Add(builder(reader));
        return results;
    }

    public static T GetRequired<T>(
        this SqliteConnection connection,
        string sql,
        Func<SqliteDataReader, T> builder)
    {
        return connection.GetRequired(sql, [], builder);
    }

    public static T GetRequired<T>(
        this SqliteConnection connection,
        string sql,
        SqliteParameter[] entries,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Get(sql, entries, builder) ?? throw new InvalidOperationException();
    }

    public static T? Get<T>(
        this SqliteConnection connection,
        string sql,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Get(sql, [], builder);
    }

    public static T? Get<T>(
        this SqliteConnection connection,
        string sql,
        SqliteParameter[] entries,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Select(sql, entries, builder).FirstOrDefault();
    }

    public static IList<T> Select<T>(
        this SqliteConnection connection,
        string sql,
        Func<SqliteDataReader, T> builder)
    {
        return connection.Select(sql, [], builder);
    }

    public static IList<T> Select<T>(
        this SqliteConnection connection,
        string sql,
        SqliteParameter[] entries,
        Func<SqliteDataReader, T> builder)
    {
        using var command = connection.CreateCommand(sql);
        command.Parameters.AddRange(entries);

        using var reader = command.ExecuteReader();
        var results = new List<T>();
        while(reader.Read()) results.Add(builder(reader));
        return results;
    }

    public static SqliteDataReader SelectRaw(this SqliteConnection connection, string sql)
    {
        return connection.SelectRaw(sql, []);
    }

    public static SqliteDataReader SelectRaw(this SqliteConnection connection, string sql, SqliteParameter[] entries)
    {
        using var command = connection.CreateCommand(sql);
        command.Parameters.AddRange(entries);

        return command.ExecuteReader();
    }

    public static T SelectRaw<T>(
        this SqliteConnection connection,
        string sql,
        Func<SqliteDataReader, T> builder)
    {
        return connection.SelectRaw(sql, [], builder);
    }

    public static T SelectRaw<T>(
        this SqliteConnection connection,
        string sql,
        SqliteParameter[] entries,
        Func<SqliteDataReader, T> builder)
    {
        using var command = connection.CreateCommand(sql);
        command.Parameters.AddRange(entries);

        using var reader = command.ExecuteReader();
        return builder(reader);
    }

    public static long Insert(this SqliteConnection connection, string table, DbPair[] entries)
    {
        var columnsClause = string.Join(",", entries.Select(static e => e.Name));
        var valuesClause = string.Join(",", entries.Select(static (e, i) => $"@{i}"));

        using var command = connection.CreateCommand($"INSERT INTO {table} ({columnsClause}) VALUES ({valuesClause})");
        foreach(var (i, entry) in entries.Index()) command.Parameters.AddWithValue($"@{i}", entry.Value ?? DBNull.Value);
        command.ExecuteNonQuery();

        using var idCommand = connection.CreateCommand("SELECT last_insert_rowid();");
        return (long)idCommand.ExecuteScalar()!;
    }

    public static void Update(
        this SqliteConnection connection,
        string table,
        DbPair idEntry,
        DbPair[] entries)
    {
        var setClause = string.Join(", ", entries.Select(static (e, i) => $"{e.Name} = @{i}"));

        using var command = connection.CreateCommand($"UPDATE {table} SET {setClause} WHERE {idEntry.Name} = @id");
        foreach(var (i, entry) in entries.Index()) command.Parameters.AddWithValue($"@{i}", entry.Value ?? DBNull.Value);
        command.Parameters.AddWithValue("@id", idEntry.Value);
        command.ExecuteNonQuery();
    }

    public static void Delete(this SqliteConnection connection, string table, DbPair idEntry)
    {
        using var command = connection.CreateCommand($"DELETE FROM {table} WHERE {idEntry.Name} = @id");
        command.Parameters.AddWithValue("@id", idEntry.Value);
        command.ExecuteNonQuery();
    }

    public static bool HasColumn(this SqliteConnection connection, string tableName, string columnName)
    {
        using var command = connection.CreateCommand($"PRAGMA table_info({tableName})");
        using var reader = command.ExecuteReader();
        while(reader.Read())
        {
            if(reader.GetString("name") == columnName) return true;
        }

        return false;
    }
}
