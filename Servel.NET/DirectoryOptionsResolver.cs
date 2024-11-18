namespace Servel.NET;

public class DirectoryOptionsResolver(IEnumerable<DirectoryOptions> directoriesOptions)
{
    private static readonly DirectoryOptions Empty = new("", new IndexParameters(0, false), null);

    public DirectoryOptions Resolve(PathString path)
    {
        foreach(var directoryOptions in directoriesOptions)
        {
            if(path.StartsWithSegments(directoryOptions.UrlPath)) return directoryOptions;
        }
        return Empty;
    }
}
