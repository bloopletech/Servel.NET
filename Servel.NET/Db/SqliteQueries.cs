using System.Data;
using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET.Db;

public class SqliteQueries(SqliteConnection connection)
{
    public long Insert(string table, SqliteParameter[] values)
    {
        var columnsClause = string.Join(",", values.Select(static e => e.ParameterName));
        var valuesClause = string.Join(",", values.Select(static (e, i) => $"@{i}"));
        var valuesArgs = values.Select((entry, i) => new SqliteParameter($"@{i}", entry.Value ?? DBNull.Value));

        using var command = CreateCommand(
            $"INSERT INTO {table} ({columnsClause}) VALUES ({valuesClause})",
            valuesArgs);
        command.ExecuteNonQuery();

        using var idCommand = CreateCommand("SELECT last_insert_rowid();");
        return (long)idCommand.ExecuteScalar()!;
    }

    public void Update(
        string table,
        SqliteParameter[] values,
        string? where = null,
        SqliteParameter[]? whereArgs = null)
    {
        var setClause = string.Join(", ", values.Select(static (e, i) => $"{e.ParameterName} = @{i}"));
        var setArgs = values.Select((entry, i) => new SqliteParameter($"@{i}", entry.Value ?? DBNull.Value));

        var whereClause = where != null ? $"WHERE {where}" : "";
        whereArgs ??= [];

        using var command = CreateCommand($"UPDATE {table} SET {setClause} {whereClause}", [..setArgs, ..whereArgs]);
        command.ExecuteNonQuery();
    }

    public void Delete(string table, string? where = null, SqliteParameter[]? whereArgs = null)
    {
        using var command = CreateCommand($"DELETE FROM {table} WHERE {where}", whereArgs);
        command.ExecuteNonQuery();
    }

    public SqliteDataReader Select(
        string table,
        string[]? columns = null,
        string? where = null,
        SqliteParameter[]? whereArgs = null,
        string? orderBy = null,
        SqliteParameter[]? orderByArgs = null,
        string? limit = null,
        SqliteParameter[]? limitArgs = null)
    {
        return Select(false, table, columns, where, whereArgs, orderBy, orderByArgs, limit, limitArgs);
    }

    /*
    (
    ...
    String groupBy, 
    String having, 
    ...
    )
    */
    public SqliteDataReader Select(
        bool distinct,
        string table,
        string[]? columns = null,
        string? where = null,
        SqliteParameter[]? whereArgs = null,
        string? orderBy = null,
        SqliteParameter[]? orderByArgs = null,
        string? limit = null,
        SqliteParameter[]? limitArgs = null)
    {
        var distinctClause = distinct ? "DISTINCT" : "";
        var columnsClause = columns != null ? string.Join(",", columns) : "*";
        var whereClause = where != null ? $"WHERE {where}" : "";
        var orderByClause = orderBy != null ? $"ORDER BY {orderBy}" : "";
        var limitClause = limit != null ? $"LIMIT {limit}" : "";

        whereArgs ??= [];
        orderByArgs ??= [];
        limitArgs ??= [];

        using var command = CreateCommand(
            $"SELECT {distinctClause} {columnsClause} FROM {table} {whereClause} {orderByClause} {limitClause}",
            [..whereArgs, ..orderByArgs, ..limitArgs]);
        return command.ExecuteReader();
    }

    public SqliteDataReader Select(string sql, SqliteParameter[]? sqlArgs = null)
    {
        using var command = CreateCommand(sql, sqlArgs);
        return command.ExecuteReader();
    }

    public int Execute(string sql, SqliteParameter[]? sqlArgs = null)
    {
        using var command = CreateCommand(sql, sqlArgs);
        return command.ExecuteNonQuery();
    }

    public T Query<T>(string sql, SqliteParameter[]? sqlArgs = null) where T : struct
    {
        using var command = CreateCommand(sql, sqlArgs);
        return (T)command.ExecuteScalar()!;
    }

    public bool HasColumn(string table, string column)
    {
        using var reader = Select($"PRAGMA table_info({table})", []);
        while(reader.Read())
        {
            if(reader.GetString("name") == column) return true;
        }

        return false;
    }

    private SqliteCommand CreateCommand(string sql, IEnumerable<SqliteParameter>? args = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        if(args != null) command.Parameters.AddRange(args);
        return command;
    }
}
