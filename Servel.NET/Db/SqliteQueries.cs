using System.Data;
using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET.Db;

public class SqliteQueries(SqliteConnection connection)
{
    public long Insert(string table, IEnumerable<SqliteParameter> values)
    {
        var columnsClause = string.Join(",", values.Select(static e => e.ParameterName));
        var valuesClause = string.Join(",", values.Select(static (e, i) => $"@{i}"));
        var valuesArgs = values.Select((entry, i) => new SqliteParameter($"@{i}", entry.Value ?? DBNull.Value));

        Execute($"INSERT INTO {table} ({columnsClause}) VALUES ({valuesClause})", valuesArgs);
        return Query<long>("SELECT last_insert_rowid();");
    }

    public void Update(
        string table,
        IEnumerable<SqliteParameter> values,
        string? where = null,
        IEnumerable<SqliteParameter>? whereArgs = null)
    {
        var setClause = string.Join(", ", values.Select(static (e, i) => $"{e.ParameterName} = @{i}"));
        var setArgs = values.Select((entry, i) => new SqliteParameter($"@{i}", entry.Value ?? DBNull.Value));

        var whereClause = where != null ? $"WHERE {where}" : "";

        Execute($"UPDATE {table} SET {setClause} {whereClause}", [..setArgs, ..whereArgs ?? []]);
    }

    public void Delete(string table, string? where = null, IEnumerable<SqliteParameter>? whereArgs = null)
    {
        Execute($"DELETE FROM {table} WHERE {where}", whereArgs);
    }

    public SqliteDataReader Select(
        string table,
        string[]? columns = null,
        string? where = null,
        string? orderBy = null,
        string? limit = null,
        IEnumerable<SqliteParameter>? args = null)
    {
        return Select(false, table, columns, where, orderBy, limit, args);
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
        string? orderBy = null,
        string? limit = null,
        IEnumerable<SqliteParameter>? args = null)
    {
        var distinctClause = distinct ? "DISTINCT" : "";
        var columnsClause = columns != null ? string.Join(",", columns) : "*";
        var whereClause = where != null ? $"WHERE {where}" : "";
        var orderByClause = orderBy != null ? $"ORDER BY {orderBy}" : "";
        var limitClause = limit != null ? $"LIMIT {limit}" : "";

        return Select(
            $"SELECT {distinctClause} {columnsClause} FROM {table} {whereClause} {orderByClause} {limitClause}",
            args);
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

    public SqliteDataReader Select(string sql, IEnumerable<SqliteParameter>? sqlArgs = null)
    {
        using var command = CreateCommand(sql, sqlArgs);
        return command.ExecuteReader();
    }

    public int Execute(string sql, IEnumerable<SqliteParameter>? sqlArgs = null)
    {
        using var command = CreateCommand(sql, sqlArgs);
        return command.ExecuteNonQuery();
    }

    public T Query<T>(string sql, IEnumerable<SqliteParameter>? sqlArgs = null) where T : struct
    {
        using var command = CreateCommand(sql, sqlArgs);
        return (T)command.ExecuteScalar()!;
    }

    private SqliteCommand CreateCommand(string sql, IEnumerable<SqliteParameter>? sqlArgs = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        if(sqlArgs != null) command.Parameters.AddRange(sqlArgs);
        return command;
    }
}
