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

    public void Update(string table, SqliteParameter[] values, string where, SqliteParameter[] whereArgs)
    {
        var setClause = string.Join(", ", values.Select(static (e, i) => $"{e.ParameterName} = @{i}"));
        var setArgs = values.Select((entry, i) => new SqliteParameter($"@{i}", entry.Value ?? DBNull.Value));

        using var command = CreateCommand($"UPDATE {table} SET {setClause} WHERE {where}", [..setArgs, ..whereArgs]);
        command.ExecuteNonQuery();
    }

    public void Delete(string table, string where, SqliteParameter[] whereArgs)
    {
        using var command = CreateCommand($"DELETE FROM {table} WHERE {where}", whereArgs);
        command.ExecuteNonQuery();
    }

    /*
    (boolean distinct,
    ...
    String groupBy, 
    String having, 
    String orderBy, 
    String limit)
    */
    public SqliteDataReader Select(string table, string[] columns, string where, SqliteParameter[] whereArgs)
    {
        var columnsClause = string.Join(",", columns);
        using var command = CreateCommand($"SELECT {columnsClause} FROM {table} WHERE {where}", whereArgs);
        return command.ExecuteReader();
    }

    public SqliteDataReader Select(string sql, SqliteParameter[] sqlArgs)
    {
        using var command = CreateCommand(sql, sqlArgs);
        return command.ExecuteReader();
    }

    public int Execute(string sql, SqliteParameter[] sqlArgs)
    {
        using var command = CreateCommand(sql, sqlArgs);
        return command.ExecuteNonQuery();
    }

    public T Query<T>(string sql, SqliteParameter[] sqlArgs) where T : struct
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
