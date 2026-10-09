namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Provider-neutral input. Adapters decide how it is serialized.</summary>
public sealed record HeadlessAiInput(string Content, IReadOnlyDictionary<string, string>? Metadata = null);
