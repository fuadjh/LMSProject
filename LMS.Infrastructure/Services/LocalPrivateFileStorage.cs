using Application.Abstractions.Learning;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public sealed class LocalPrivateFileStorage
    : IPrivateFileStorage
{
    private readonly string _rootPath;

    public LocalPrivateFileStorage(
        IConfiguration configuration)
    {
        var configuredPath =
            configuration["Storage:PrivateRoot"];

        _rootPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(
                AppContext.BaseDirectory,
                "private-storage")
            : Path.GetFullPath(configuredPath);

        Directory.CreateDirectory(_rootPath);
    }

    public async Task SaveAsync(
        string storageKey,
        Stream source,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafeFullPath(storageKey);

        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException(
                "Storage directory is invalid.");

        Directory.CreateDirectory(directory);

        await using var target = File.Create(fullPath);

        await source.CopyToAsync(
            target,
            cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafeFullPath(storageKey);

        Stream? stream = File.Exists(fullPath)
            ? new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan)
            : null;

        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            File.Exists(GetSafeFullPath(storageKey)));
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafeFullPath(storageKey);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string GetSafeFullPath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException(
                "Storage key is required.");

        var normalized = storageKey
            .Replace('\\', '/')
            .TrimStart('/');

        var fullPath = Path.GetFullPath(
            Path.Combine(
                _rootPath,
                normalized.Replace(
                    '/',
                    Path.DirectorySeparatorChar)));

        if (!fullPath.StartsWith(
                _rootPath,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid storage path.");
        }

        return fullPath;
    }
}