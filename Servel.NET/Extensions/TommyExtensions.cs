using Tommy;

namespace Servel.NET.Extensions;

public static class TommyExtensions
{
    public static TomlNode? Get(this TomlTable table, string key) => table.RawTable.GetValueOrDefault(key);
    public static TomlNode GetRequired(this TomlTable table, string key) => table.RawTable[key];

    public static string? GetString(this TomlTable table, string key) => table.Get(key)?.AsString?.Value;
    public static string GetRequiredString(this TomlTable table, string key) => table.GetRequired(key).AsString!.Value;

    public static int? GetInteger(this TomlTable table, string key) => (int?)table.Get(key)?.AsInteger?.Value;

    public static bool? GetBoolean(this TomlTable table, string key) => table.Get(key)?.AsBoolean?.Value;

    public static TomlArray? GetArray(this TomlTable table, string key) => (TomlArray?)table.Get(key);
    public static TomlArray GetRequiredArray(this TomlTable table, string key) => (TomlArray)table.GetRequired(key);

    public static T? GetEnum<T>(this TomlTable table, string key) where T : struct
    {
        var value = table.GetString(key);
        if(value == null) return null;
        return Enum.Parse<T>(value);
    }
}
