namespace Servel.NET;

// Based on https://stackoverflow.com/a/50646048
public static class Logging
{
    public static ILoggerFactory LoggerFactory { get; set; } = null!;

    public static ILogger<T> Create<T>() => LoggerFactory.CreateLogger<T>();
    public static ILogger Create(string categoryName) => LoggerFactory.CreateLogger(categoryName);
}
