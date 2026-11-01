using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Provider-neutral input. Adapters decide how it is serialized.</summary>
/// <remarks>Input content and metadata are snapshotted and bounded whenever the value is created or copied with an initializer.</remarks>
public sealed record HeadlessAiInput
{
    /// <summary>Maximum input content length, in UTF-16 characters.</summary>
    public const int MaxContentCharacters = 1_048_576;
    /// <summary>Maximum number of metadata entries.</summary>
    public const int MaxMetadataEntries = 128;
    /// <summary>Maximum metadata key length, in UTF-16 characters.</summary>
    public const int MaxMetadataKeyCharacters = 256;
    /// <summary>Maximum metadata value length, in UTF-16 characters.</summary>
    public const int MaxMetadataValueCharacters = 8_192;

    private string _content = string.Empty;
    private IReadOnlyDictionary<string, string> _metadata =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal));

    /// <summary>Creates an input snapshot after validating and copying its content and metadata.</summary>
    /// <param name="content">Provider-neutral input content.</param>
    /// <param name="metadata">Optional metadata to snapshot; null becomes an empty immutable dictionary.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
    /// <exception cref="ArgumentException">Content or metadata exceeds the documented bounds, or metadata contains invalid keys/values.</exception>
    public HeadlessAiInput(string content, IReadOnlyDictionary<string, string>? metadata = null)
    {
        Content = content;
        Metadata = metadata!;
    }

    /// <summary>Gets or initializes the input content, bounded by <see cref="MaxContentCharacters"/>.</summary>
    public string Content
    {
        get => _content;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Length > MaxContentCharacters)
                throw new ArgumentException($"Input content cannot exceed {MaxContentCharacters} characters.", nameof(value));
            _content = value;
        }
    }

    /// <summary>Gets or initializes an immutable metadata snapshot.</summary>
    public IReadOnlyDictionary<string, string> Metadata
    {
        get => _metadata;
        init => _metadata = SnapshotMetadata(value);
    }

    private static IReadOnlyDictionary<string, string> SnapshotMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        var copy = new Dictionary<string, string>(StringComparer.Ordinal);
        if (metadata is not null)
        {
            foreach (var pair in metadata)
            {
                if (copy.Count >= MaxMetadataEntries)
                    throw new ArgumentException($"Input metadata cannot contain more than {MaxMetadataEntries} entries.", nameof(metadata));
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > MaxMetadataKeyCharacters)
                    throw new ArgumentException($"Metadata keys must contain 1–{MaxMetadataKeyCharacters} non-whitespace characters.", nameof(metadata));
                if (pair.Value is null || pair.Value.Length > MaxMetadataValueCharacters)
                    throw new ArgumentException($"Metadata values cannot exceed {MaxMetadataValueCharacters} characters or be null.", nameof(metadata));
                if (!copy.TryAdd(pair.Key, pair.Value))
                    throw new ArgumentException($"Input metadata contains duplicate key '{pair.Key}'.", nameof(metadata));
            }
        }

        return new ReadOnlyDictionary<string, string>(copy);
    }

    /// <summary>Deconstructs the input into its content and immutable metadata snapshot.</summary>
    public void Deconstruct(out string content, out IReadOnlyDictionary<string, string> metadata)
    {
        content = Content;
        metadata = Metadata;
    }
}
