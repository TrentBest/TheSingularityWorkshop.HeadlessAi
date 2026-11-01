using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Provider-neutral input. Adapters decide how it is serialized.</summary>
/// <remarks>Input content and metadata are copied and bounded when the value is created.</remarks>
public sealed record HeadlessAiInput
{
    public const int MaxContentCharacters = 1_048_576;
    public const int MaxMetadataEntries = 128;
    public const int MaxMetadataKeyCharacters = 256;
    public const int MaxMetadataValueCharacters = 8_192;

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

    public string Content { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
