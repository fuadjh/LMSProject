using System.Diagnostics;
using Application.Abstractions.Learning;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public sealed class FfmpegVideoTranscoder
    : IVideoTranscoder
{
    private readonly IPrivateFileStorage _storage;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public FfmpegVideoTranscoder(
        IPrivateFileStorage storage,
        IConfiguration configuration)
    {
        _storage = storage;

        _ffmpegPath =
            configuration["Video:FfmpegPath"]
            ?? "ffmpeg";

        _ffprobePath =
            configuration["Video:FfprobePath"]
            ?? "ffprobe";
    }

    public async Task<VideoTranscodeResult> TranscodeAsync(
        Stream source,
        string sourceExtension,
        Stream? cover,
        string? coverExtension,
        CancellationToken cancellationToken = default)
    {
        var operationId = Guid.NewGuid().ToString("N");

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "lms-video",
            operationId);

        var outputFolder = Path.Combine(
            tempRoot,
            "hls");

        Directory.CreateDirectory(outputFolder);

        var inputPath = Path.Combine(
            tempRoot,
            $"source{sourceExtension}");

        try
        {
            await using (var inputFile =
                         File.Create(inputPath))
            {
                await source.CopyToAsync(
                    inputFile,
                    cancellationToken);
            }

            var playlistPath = Path.Combine(
                outputFolder,
                "index.m3u8");

            var segmentPattern = Path.Combine(
                outputFolder,
                "segment_%05d.ts");

            await RunFfmpegAsync(
                inputPath,
                playlistPath,
                segmentPattern,
                cancellationToken);

            var duration =
                await ReadDurationAsync(
                    inputPath,
                    cancellationToken);

            var storagePrefix =
                $"learning/videos/{DateTime.UtcNow:yyyy/MM}/" +
                operationId;

            foreach (var file in Directory
                         .EnumerateFiles(outputFolder))
            {
                await using var fileStream =
                    File.OpenRead(file);

                await _storage.SaveAsync(
                    $"{storagePrefix}/{Path.GetFileName(file)}",
                    fileStream,
                    cancellationToken);
            }

            string? coverStorageKey = null;

            if (cover is not null &&
                !string.IsNullOrWhiteSpace(coverExtension))
            {
                var normalizedCoverExtension =
                    coverExtension.ToLowerInvariant();

                if (normalizedCoverExtension is not
                    (".jpg" or ".jpeg" or ".png" or ".webp"))
                {
                    throw new InvalidOperationException(
                        "فرمت تصویر کاور معتبر نیست.");
                }

                coverStorageKey =
                    $"{storagePrefix}/cover" +
                    normalizedCoverExtension;

                await _storage.SaveAsync(
                    coverStorageKey,
                    cover,
                    cancellationToken);
            }

            return new VideoTranscodeResult(
                $"{storagePrefix}/index.m3u8",
                coverStorageKey,
                duration);
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, true);
            }
            catch
            {
                // Temporary cleanup failure is not fatal.
            }
        }
    }

    private async Task RunFfmpegAsync(
        string inputPath,
        string playlistPath,
        string segmentPattern,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-y");
        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(inputPath);

        startInfo.ArgumentList.Add("-map");
        startInfo.ArgumentList.Add("0:v:0");

        startInfo.ArgumentList.Add("-map");
        startInfo.ArgumentList.Add("0:a?");

        startInfo.ArgumentList.Add("-c:v");
        startInfo.ArgumentList.Add("libx264");

        startInfo.ArgumentList.Add("-preset");
        startInfo.ArgumentList.Add("veryfast");

        startInfo.ArgumentList.Add("-crf");
        startInfo.ArgumentList.Add("23");

        startInfo.ArgumentList.Add("-c:a");
        startInfo.ArgumentList.Add("aac");

        startInfo.ArgumentList.Add("-b:a");
        startInfo.ArgumentList.Add("128k");

        startInfo.ArgumentList.Add("-hls_time");
        startInfo.ArgumentList.Add("6");

        startInfo.ArgumentList.Add("-hls_playlist_type");
        startInfo.ArgumentList.Add("vod");

        startInfo.ArgumentList.Add("-hls_segment_filename");
        startInfo.ArgumentList.Add(segmentPattern);

        startInfo.ArgumentList.Add(playlistPath);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var errorTask =
            process.StandardError.ReadToEndAsync(
                cancellationToken);

        await process.WaitForExitAsync(
            cancellationToken);

        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"FFmpeg failed: {error}");
        }
    }

    private async Task<int?> ReadDurationAsync(
        string inputPath,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ffprobePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-v");
        startInfo.ArgumentList.Add("error");

        startInfo.ArgumentList.Add("-show_entries");
        startInfo.ArgumentList.Add("format=duration");

        startInfo.ArgumentList.Add("-of");
        startInfo.ArgumentList.Add(
            "default=noprint_wrappers=1:nokey=1");

        startInfo.ArgumentList.Add(inputPath);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var output =
            await process.StandardOutput
                .ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(
            cancellationToken);

        if (process.ExitCode != 0)
            return null;

        if (!double.TryParse(
                output.Trim(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var seconds))
        {
            return null;
        }

        return Math.Max(0, (int)Math.Round(seconds));
    }
}