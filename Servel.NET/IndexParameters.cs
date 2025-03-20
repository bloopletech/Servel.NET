using Servel.NET.Extensions;

namespace Servel.NET;

public readonly record struct IndexParameters(uint Depth, bool CountChildren)
{
    public static IndexParameters? Parse(HttpRequest httpRequest)
    {
        var success = true;
        success &= uint.TryParse(httpRequest.Param("depth"), out var depth);
        success &= bool.TryParse(httpRequest.Param("countChildren"), out var countChildren);

        return success ? new IndexParameters(depth, countChildren) : null;
    }
}