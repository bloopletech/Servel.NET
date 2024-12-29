using System.Collections.Concurrent;
using Microsoft.Extensions.FileProviders.Physical;
using Servel.NET.Models;

namespace Servel.NET.Services;

public class ThumbnailService(CacheDatabaseService databaseService)
{
    private ThumbnailGenerator thumbnailGenerator = new ThumbnailGenerator();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> pathLocks = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> driveLocks = new();

    public static async Task ThumbnailDirectoryBackground(IBackgroundTaskQueue queue, PhysicalDirectoryInfo directoryInfo)
    {
        await queue.QueueAsync(async (services, _) =>
        {
            foreach(var physicalFileInfo in directoryInfo.OfType<PhysicalFileInfo>())
            {
                await ThumbnailFileBackground(queue, physicalFileInfo);
            }
        });
    }

    public static async Task ThumbnailFileBackground(IBackgroundTaskQueue queue, PhysicalFileInfo fileInfo)
    {
        await queue.QueueAsync(async (services, _) =>
        {
            var thumbnailsService = services.GetRequiredService<ThumbnailService>();
            thumbnailsService.FindOrCreateByPath(fileInfo);
        });
    }

    public byte[]? FindOrCreateByPath(PhysicalFileInfo fileInfo)
    {
        if(FindByPath(fileInfo, out var existingData)) return existingData;

        var path = fileInfo.PhysicalPath;
        var pathLock = pathLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
        pathLock.Wait();

        try
        {
            return FindOrCreateByPathRacy(fileInfo);
        }
        finally
        {
            pathLocks.TryRemove(path, out _);
            pathLock.Release();
        }
    }

    private byte[]? FindOrCreateByPathRacy(PhysicalFileInfo fileInfo)
    {
        if(FindByPath(fileInfo, out var existingData)) return existingData;

        var data = GenerateThumbnail(fileInfo.PhysicalPath);

        var thumbnail = new Thumbnail
        {
            Path = fileInfo.PhysicalPath,
            Data = data,
            Mtime = fileInfo.LastModified.ToUnixTimeMilliseconds()
        };

        using var db = databaseService.Connect();
        thumbnail.Save(db);

        return data;
    }

    private bool FindByPath(PhysicalFileInfo fileInfo, out byte[]? data)
    {
        using var db = databaseService.Connect();
        var existing = Thumbnail.FindByPath(db, fileInfo.PhysicalPath);
        if(existing != null && existing.Mtime == fileInfo.LastModified.ToUnixTimeMilliseconds())
        {
            data = existing.Data;
            return true;
        }

        data = null;
        return false;
    }

    private byte[]? GenerateThumbnail(string path)
    {
        var pathRoot = Path.GetPathRoot(path) ?? "";
        var driveLock = driveLocks.GetOrAdd(pathRoot, _ => new SemaphoreSlim(1, 1));
        driveLock.Wait();

        try
        {
            return thumbnailGenerator.Thumbnail(path);
        }
        finally
        {
            driveLocks.TryRemove(pathRoot, out _);
            driveLock.Release();
        }
    }
}
