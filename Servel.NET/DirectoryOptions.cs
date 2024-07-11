namespace Servel.NET;

public readonly record struct DirectoryOptions(
    PathString UrlPath,
    IndexParameters DefaultParameters,
    ListingQuery? DefaultQuery);