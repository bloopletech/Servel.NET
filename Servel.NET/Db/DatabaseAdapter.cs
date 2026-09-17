using Microsoft.Data.Sqlite;
using Servel.NET.Extensions;

namespace Servel.NET.Db;

/*
package net.bloople.manga.db

import android.content.ContentValues
import android.database.Cursor
import android.database.sqlite.SQLiteDatabase

class DatabaseAdapter(private val db: SQLiteDatabase, private val table: String) {
}
*/
public class DatabaseAdapter<T>(SqliteConnection db, string table) where T : IModel<T>
{
    public long Insert(SqliteParameter[] values)
    {
        //return db.insert(table, null, values)
        return db.Insert(table, values);
    }

    public void Update(SqliteParameter[] values, long id)
    {
        //db.update(table, values, "_id=?", arrayOf(id.toString()))
        db.Update(table, new SqliteParameter("Id", id), values);
    }

    public void Delete(long id)
    {
        //db.delete(table, "_id=?", arrayOf(id.toString()))
        db.Delete(table, new SqliteParameter("Id", id));
    }

/*
    fun <T> find(id: Long, build: (Cursor) -> T): T {
        db.rawQuery("SELECT * FROM $table WHERE _id=?", arrayOf(id.toString())).use {
            it.moveToFirst()
            return if (it.count > 0) build(it) else throw NoSuchElementException("$table with id $id not found")
        }
    }
*/
    public T Find(long id, Func<SqliteDataReader, T> builder)
    {
        return db.GetRequired(table, new SqliteParameter("Id", id), builder);
    }

/*
    fun <T> find(query: String, vararg args: String, build: (Cursor) -> T): T {
        db.rawQuery(query, args).use {
            it.moveToFirst()
            return if (it.count > 0) build(it) else throw NoSuchElementException("$table record not found")
        }
    }
*/
    public T Find(string query, SqliteParameter[] args, Func<SqliteDataReader, T> builder)
    {
        return db.GetRequired(query, args, builder);
    }

/*
    fun <T> findBy(query: String, vararg args: String, build: (Cursor) -> T): T? {
        db.rawQuery(query, args).use {
            it.moveToFirst()
            return if (it.count > 0) build(it) else null
        }
    }
*/
    public T? FindBy(string query, SqliteParameter[] args, Func<SqliteDataReader, T> builder)
    {
        return db.Get(query, args, builder);
    }

/*
    fun <T> query(query: String, vararg args: String, process: (Cursor) -> T): T {
        db.rawQuery(query, args).use { return process(it) }
    }
*/
    public T Query(string query, SqliteParameter[] args, Func<SqliteDataReader, T> processor)
    {
        return db.SelectRaw(query, args, processor);
    }

/*
    fun count(): Long {
        db.rawQuery("SELECT COUNT(*) FROM $table", null).use {
            it.moveToFirst()
            return it.getLong(0)
        }
    }
*/
    public long Count()
    {
        //using var reader = db.SelectRaw($"SELECT COUNT(*) FROM {table}");
        using var command = db.CreateCommand($"SELECT COUNT(*) FROM {table}");
        return (long)command.ExecuteScalar()!;
    }
}
