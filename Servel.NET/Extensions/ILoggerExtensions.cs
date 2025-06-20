namespace Servel.NET.Extensions;

public static class ILoggerExtensions
{
    public static void Critical(this ILogger logger, string message) => logger.LogCritical("{Message}", message);
    public static void Debug(this ILogger logger, string message) => logger.LogDebug("{Message}", message);
    public static void Error(this ILogger logger, string message) => logger.LogError("{Message}", message);
    public static void Information(this ILogger logger, string message) => logger.LogInformation("{Message}", message);
    public static void Trace(this ILogger logger, string message) => logger.LogTrace("{Message}", message);
    public static void Warning(this ILogger logger, string message) => logger.LogWarning("{Message}", message);

    public static IMeasurer Measure(this ILogger logger)
    {
        if(logger.IsEnabled(LogLevel.Information)) return new Measurer(logger, null);
        return new NullMeasurer();
    }

    public static IMeasurer Measure(this ILogger logger, string context)
    {
        if(logger.IsEnabled(LogLevel.Information)) return new Measurer(logger, null, context);
        return new NullMeasurer();
    }

    public static void Measure(this ILogger logger, Action callback)
    {
        using(logger.Measure()) callback();
    }

    public static void Measure(this ILogger logger, string context, Action callback)
    {
        using(logger.Measure(context)) callback();
    }

    public static T Measure<T>(this ILogger logger, Func<T> callback)
    {
        using(logger.Measure()) return callback();
    }

    public static T Measure<T>(this ILogger logger, string context, Func<T> callback)
    {
        using(logger.Measure(context)) return callback();
    }

    public static async Task Measure(this ILogger logger, Func<Task> callback)
    {
        using(logger.Measure()) await callback();
    }

    public static async Task Measure(this ILogger logger, string context, Func<Task> callback)
    {
        using(logger.Measure(context)) await callback();
    }

    public static async Task<T> Measure<T>(this ILogger logger, Func<Task<T>> callback)
    {
        using(logger.Measure()) return await callback();
    }

    public static async Task<T> Measure<T>(this ILogger logger, string context, Func<Task<T>> callback)
    {
        using(logger.Measure(context)) return await callback();
    }

    public static IMeasurer BeginMeasureScope(this ILogger logger, string messageFormat, params object?[] args)
    {
        var scope = logger.BeginScope(messageFormat, args);
        if(logger.IsEnabled(LogLevel.Information)) return new Measurer(logger, scope);
        return new LoggerScopeNullMeasurer(scope);
    }
}
