namespace Application.Abstractions.Learning;

public interface IVideoTranscoder
{
    Task<VideoTranscodeResult> TranscodeAsync(
        Stream source,
        string sourceExtension,
        Stream? cover,
        string? coverExtension,
        CancellationToken cancellationToken = default);
}

public sealed record VideoTranscodeResult(
    string MasterPlaylistKey,
    string? CoverStorageKey,
    int? DurationSeconds);