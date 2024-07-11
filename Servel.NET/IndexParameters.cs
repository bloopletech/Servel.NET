namespace Servel.NET;

public readonly record struct IndexParameters(uint Depth, bool CountChildren)
{
    public static IndexParameters? Parse(HttpRequest httpRequest)
    {
        var success = true;
        var depthStr = httpRequest.Query["depth"];
        success &= uint.TryParse(depthStr.ToString(), out var depth);

        var countChildrenStr = httpRequest.Query["countChildren"];
        success &= bool.TryParse(countChildrenStr, out var countChildren);

        return success ? new IndexParameters(depth, countChildren) : null;
    }
}