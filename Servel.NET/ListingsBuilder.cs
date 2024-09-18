using System.Web;

namespace Servel.NET;

public readonly struct ListingsBuilder(string BasePath)
{
    public readonly List<Listing> Listings = [];

    public IEnumerable<Listing> Build(SiteListingOption[] options)
    {
        Listings.Clear();

        foreach (var option in options)
        {
            if (option.IsRootWildcard)
            {
                AddDriveLetters(option.Url, option.ShowVolumeLabels ?? false);
            }
            else
            {
                Listings.Add(new Listing(Path.GetFullPath(option.Dir, BasePath), option.Url, option.Name));
            }
        }

        return Listings;
    }

    private void AddDriveLetters(string baseUrlPath, bool showVolumeLabels)
    {
        if (baseUrlPath.EndsWith("/*")) baseUrlPath = baseUrlPath[..^1];
        baseUrlPath = baseUrlPath.EnsureTrailingSlash();

        var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

        foreach (var drive in drives)
        {
            var fsPath = drive.RootDirectory.FullName;
            var urlPath = $"{baseUrlPath}{HttpUtility.UrlPathEncode(drive.Name[0..1])}";
            var name = showVolumeLabels ? drive.VolumeLabel : null;
            Listings.Add(new Listing(fsPath, urlPath, name));
        }
    }
}
