using System.Collections.ObjectModel;
using System.Net;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Normalized response content and transport metadata.</summary>
/// <remarks>Content and optional metadata are bounded and snapshotted whenever this value is constructed or copied.</remarks>
public sealed record HeadlessAiOutput
{
    /// <summary>Maximum normalized content length, in UTF-16 characters.</summary>
    public const int MaxContentCharacters = 64 * 1024 * 1024;
    /// <summary>Maximum number of optional metadata entries.</summary>
    public const int MaxMetadataEntries = 128;
    /// <summary>Maximum metadata key length, in UTF-16 characters.</summary>
    public const int MaxMetadataKeyCharacters = 256;
    /// <summary>Maximum metadata value length, in UTF-16 characters.</summary>
    public const int MaxMetadataValueCharacters = 8_192;
    /// <summary>Maximum aggregate metadata value length, in UTF-16 characters.</summary>
    public const int MaxTotalMetadataValueCharacters = 262_144;
    /// <summary>Maximum media type length, in UTF-16 characters.</summary>
    public const int MaxMediaTypeCharacters = 256;

    private string _content = string.Empty;
    private string? _mediaType;
    private IReadOnlyDictionary<string, string>? _metadata;

    /// <summary>Creates a normalized output snapshot.</summary>
    /// <param name="content">Adapter-extracted content. Empty content is representable; adapters decide whether it is meaningful.</param>
    /// <param name="statusCode">HTTP response status code.</param>
    /// <param name="mediaType">Optional response media type.</param>
    /// <param name="metadata">Optional adapter-extracted metadata, copied into an immutable snapshot.</param>
    public HeadlessAiOutput(string content, HttpStatusCode statusCode, string? mediaType,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Content = content;
        StatusCode = statusCode;
        MediaType = mediaType;
        Metadata = metadata;
    }

    /// <summary>Gets or initializes normalized response content.</summary>
    public string Content
    {
        get => _content;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Length > MaxContentCharacters)
                throw new ArgumentException($"Output content cannot exceed {MaxContentCharacters} characters.", nameof(value));
            _content = value;
        }
    }

    /// <summary>Gets or initializes the HTTP response status code.</summary>
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>Gets or initializes the optional response media type.</summary>
    public string? MediaType
    {
        get => _mediaType;
        init
        {
            if (value is not null &&
                (value.Length > MaxMediaTypeCharacters || value.Any(character => char.IsControl(character))))
                throw new ArgumentException($"Media types must be at most {MaxMediaTypeCharacters} characters and contain no control characters.", nameof(value));
            _mediaType = value;
        }
    }

    /// <summary>Gets or initializes optional immutable adapter metadata.</summary>
    public IReadOnlyDictionary<string, string>? Metadata
    {
        get => _metadata;
        init => _metadata = SnapshotMetadata(value);
    }

    private static IReadOnlyDictionary<string, string>? SnapshotMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null)
            return null;

        var copy = new Dictionary<string, string>(StringComparer.Ordinal);
        long totalValueCharacters = 0;
        foreach (var pair in metadata)
        {
            if (copy.Count >= MaxMetadataEntries)
                throw new ArgumentException($"Output metadata cannot contain more than {MaxMetadataEntries} entries.", nameof(metadata));
            if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > MaxMetadataKeyCharacters)
                throw new ArgumentException($"Output metadata keys must contain 1–{MaxMetadataKeyCharacters} non-whitespace characters.", nameof(metadata));
            if (pair.Value is null || pair.Value.Length > MaxMetadataValueCharacters)
                throw new ArgumentException($"Output metadata values cannot exceed {MaxMetadataValueCharacters} characters or be null.", nameof(metadata));
            totalValueCharacters += pair.Value.Length;
            if (totalValueCharacters > MaxTotalMetadataValueCharacters)
                throw new ArgumentException($"Output metadata cannot contain more than {MaxTotalMetadataValueCharacters} value characters in total.", nameof(metadata));
            if (!copy.TryAdd(pair.Key, pair.Value))
                throw new ArgumentException($"Output metadata contains duplicate key '{pair.Key}'.", nameof(metadata));
        }

        return new ReadOnlyDictionary<string, string>(copy);
    }

    /// <summary>Deconstructs the output into its normalized content, status, media type, and metadata.</summary>
    public void Deconstruct(out string content, out HttpStatusCode statusCode, out string? mediaType,
        out IReadOnlyDictionary<string, string>? metadata)
    {
        content = Content;
        statusCode = StatusCode;
        mediaType = MediaType;
        metadata = Metadata;
    }
}
