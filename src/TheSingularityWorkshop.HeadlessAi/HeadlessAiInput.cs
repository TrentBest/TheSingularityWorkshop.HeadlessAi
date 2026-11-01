using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Provider-neutral input. Adapters decide how it is serialized.</summary>
/// <remarks>Input content and metadata are copied and bounded when the value is created.</remarks>
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

    /// <summary>Creates an input snapshot after validating and copying its content and metadata.</summary>
    /// <param name="content">Provider-neutral input content.</param>
    /// <param name="metadata">Optional metadata to snapshot; null becomes an empty immutable dictionary.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
    /// <exception cref="ArgumentException">Content or metadata exceeds the documented bounds, or metadata contains invalid/duplicate keys.</exception>
    public HeadlessAiInput(string content, IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length > MaxContentCharacters)
            throw new ArgumentException($"Input content cannot exceed {MaxContentCharacters} characters.", nameof(content));

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

        Content = content;
        Metadata = new ReadOnlyDictionary<string, string>(copy);
    }

    /// <summary>Gets the immutable input content.</summary>
    public string Content { get; }

    /// <summary>Gets the immutable metadata snapshot, empty when no metadata was supplied.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>Deconstructs the input into its content and immutable metadata snapshot.</summary>
    public void Deconstruct(out string content, out IReadOnlyDictionary<string, string> metadata)
    {
        content = Content;
        metadata = Metadata;
    }
}
