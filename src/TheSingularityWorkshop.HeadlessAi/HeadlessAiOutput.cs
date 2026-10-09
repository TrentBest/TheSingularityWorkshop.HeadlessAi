namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Normalized response content and transport metadata.</summary>
public sealed record HeadlessAiOutput(string Content, HttpStatusCode StatusCode, string? MediaType,
    IReadOnlyDictionary<string, string>? Metadata = null);
