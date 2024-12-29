using System.Data;
using System.Data.Common;

namespace Servel.NET.Extensions;

public static class DbDataReaderExtensions
{
    public static byte[]? GetByteArray(this DbDataReader reader, string name)
    {
        if(reader.IsDBNull(name)) return null;

        using var readerStream = reader.GetStream(name);
        using var memoryStream = new MemoryStream();
        readerStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
