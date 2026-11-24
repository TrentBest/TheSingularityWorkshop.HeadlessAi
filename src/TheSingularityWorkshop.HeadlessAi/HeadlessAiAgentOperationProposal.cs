namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// One structured operation suggested by a model. It is an untrusted proposal only:
/// the consuming host must validate the operation against its closed action vocabulary,
/// current task scope, capabilities, and human-approval policy before doing anything.
/// </summary>
public sealed record HeadlessAiAgentOperationProposal
{
    /// <summary>Host-recognized operation identifier proposed by the model; it grants no permission.</summary>
    public required string Operation { get; init; }
    /// <summary>Human-readable summary of the proposed operation.</summary>
    public required string Summary { get; init; }
    /// <summary>Model-provided rationale, retained as untrusted explanatory text.</summary>
    public required string Rationale { get; init; }
    /// <summary>Exact JSON-object arguments for host-side validation and authorization.</summary>
    public required string ArgumentsJson { get; init; }

    /// <summary>Validates bounds and JSON shape without authorizing or executing the proposal.</summary>
    public void Validate()
    {
        static void Required(string value, string name, int maximum)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > maximum || value.Any(char.IsControl))
                throw new ArgumentException($"{name} must be non-empty, control-free, and at most {maximum} characters.", name);
        }

        Required(Operation, nameof(Operation), 128);
        Required(Summary, nameof(Summary), 1024);
        Required(Rationale, nameof(Rationale), 4096);
        if (string.IsNullOrWhiteSpace(ArgumentsJson) || ArgumentsJson.Length > 64 * 1024)
            throw new ArgumentException("Operation arguments must be non-empty and at most 64 KiB.", nameof(ArgumentsJson));
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(ArgumentsJson,
                new System.Text.Json.JsonDocumentOptions { MaxDepth = 16 });
            if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
                throw new ArgumentException("Operation arguments must be a JSON object.", nameof(ArgumentsJson));
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new ArgumentException("Operation arguments must be valid JSON.", nameof(ArgumentsJson), exception);
        }
    }
}
