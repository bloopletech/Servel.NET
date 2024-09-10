using Tommy;

namespace Servel.NET;

public static class TommyExtensions
{
    public static string? GetString(this TomlNode node, string key)
    {
        return node[key]?.AsString?.Value;
    }

    public static int? GetInteger(this TomlNode node, string key)
    {
        return (int?)node[key]?.AsInteger?.Value;
    }

    public static bool? GetBoolean(this TomlNode node, string key)
    {
        return node[key]?.AsBoolean?.Value;
    }

    public static TomlArray? GetArray(this TomlNode node, string key)
    {
        return node[key]?.AsArray!;
    }

    public static TomlTable? GetSubTable(this TomlNode node, string key)
    {
        return node[key]?.AsTable!;
    }

    public static T? GetEnum<T>(this TomlNode node, string key) where T : struct
    {
        var value = GetString(node, key);
        if (value == null) return null;
        return Enum.Parse<T>(value);
    }
}
