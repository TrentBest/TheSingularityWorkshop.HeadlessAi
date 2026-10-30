namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// A provider-neutral record of one multi-provider comparison. It preserves independent provider
/// results and explicitly attributed assessments; it does not calculate consensus or select a winner.
/// </summary>
public sealed record HeadlessAiAgentTurnComparison
{
    /// <summary>Current comparison schema version.</summary>
    public const int CurrentVersion = 1;

    /// <summary>Comparison schema version.</summary>
    public int Version { get; init; } = CurrentVersion;
    /// <summary>Host-owned task identifier shared by every provider result.</summary>
    public required string TaskId { get; init; }
    /// <summary>Host-owned task-definition version shared by every provider result, when supplied.</summary>
    public string? TaskVersion { get; init; }
    /// <summary>Correlation identifier shared by every provider result.</summary>
    public required string CorrelationId { get; init; }
    /// <summary>Repository identity in owner/name form.</summary>
    public required string Repository { get; init; }
    /// <summary>Candidate SHA supplied to every provider, when present.</summary>
    public string? CandidateCommitSha { get; init; }
    /// <summary>Fingerprint of the exact canonical request package sent to every provider.</summary>
    public required string RequestFingerprint { get; init; }
    /// <summary>Independent provider results, including normalized failures.</summary>
    public IReadOnlyList<HeadlessAiAgentTurnResult> ProviderResults { get; init; } = Array.Empty<HeadlessAiAgentTurnResult>();
    /// <summary>Provider-attributed assessments; failed providers may have no assessment.</summary>
    public IReadOnlyList<HeadlessAiProviderAssessment> Assessments { get; init; } = Array.Empty<HeadlessAiProviderAssessment>();
    /// <summary>Explicitly attributed agreement and disagreement relationships between findings.</summary>
    public IReadOnlyList<HeadlessAiAgentTurnFindingRelationship> FindingRelationships { get; init; } = Array.Empty<HeadlessAiAgentTurnFindingRelationship>();

    /// <summary>
    /// Validates shared request provenance and all provider attributions. Validation never infers
    /// agreement, resolves disagreement, or upgrades a model statement into a verified fact.
    /// </summary>
    public void Validate()
    {
        if (Version != CurrentVersion)
            throw new ArgumentOutOfRangeException(nameof(Version), $"Unsupported comparison version: {Version}.");
        Require(TaskId, nameof(TaskId), 128);
        if (TaskVersion is not null) Require(TaskVersion, nameof(TaskVersion), 128);
        Require(CorrelationId, nameof(CorrelationId), 128);
        Require(Repository, nameof(Repository), 256);
        Require(RequestFingerprint, nameof(RequestFingerprint), 128);
        if (CandidateCommitSha is not null && !IsSha(CandidateCommitSha))
            throw new ArgumentException("Candidate commit SHA must be a full 40-character SHA.", nameof(CandidateCommitSha));
        ArgumentNullException.ThrowIfNull(ProviderResults);
        ArgumentNullException.ThrowIfNull(Assessments);
        ArgumentNullException.ThrowIfNull(FindingRelationships);
        if (ProviderResults.Count == 0 || ProviderResults.Count > 32)
            throw new ArgumentException("A comparison must contain between one and 32 provider results.", nameof(ProviderResults));

        var resultsByTurn = new Dictionary<string, HeadlessAiAgentTurnResult>(StringComparer.Ordinal);
        var providerIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var result in ProviderResults)
        {
            ArgumentNullException.ThrowIfNull(result);
            result.Validate();
            if (!string.Equals(result.TaskId, TaskId, StringComparison.Ordinal) ||
                !string.Equals(result.TaskVersion, TaskVersion, StringComparison.Ordinal) ||
                !string.Equals(result.CorrelationId, CorrelationId, StringComparison.Ordinal) ||
                !string.Equals(result.Repository, Repository, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(result.RequestFingerprint, RequestFingerprint, StringComparison.Ordinal) ||
                !string.Equals(result.CandidateCommitSha, CandidateCommitSha, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Every provider result must belong to the same task, task version, correlation, repository, candidate, and canonical request.");
            if (!providerIds.Add(result.ProviderId))
                throw new ArgumentException("A provider may appear only once in a comparison.", nameof(ProviderResults));
            if (!resultsByTurn.TryAdd(result.TurnId, result))
                throw new ArgumentException("Provider turn IDs must be unique within a comparison.", nameof(ProviderResults));
        }

        var findingsById = new Dictionary<string, HeadlessAiProviderFinding>(StringComparer.Ordinal);
        var assessedProviders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var assessment in Assessments)
        {
            ArgumentNullException.ThrowIfNull(assessment);
            assessment.Validate();
            if (!resultsByTurn.TryGetValue(assessment.TurnId, out var result) ||
                !string.Equals(result.ProviderId, assessment.ProviderId, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Each assessment must identify an included provider result by turn and provider.", nameof(Assessments));
            if (!assessedProviders.Add(assessment.ProviderId))
                throw new ArgumentException("A provider may have only one assessment.", nameof(Assessments));
            foreach (var finding in assessment.Findings)
            {
                if (!findingsById.TryAdd(finding.FindingId, finding))
                    throw new ArgumentException("Finding IDs must be unique within a comparison.", nameof(Assessments));
            }
        }

        foreach (var relationship in FindingRelationships)
        {
            ArgumentNullException.ThrowIfNull(relationship);
            relationship.Validate(findingsById);
        }
    }

    private static void Require(string value, string name, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max)
            throw new ArgumentException($"{name} must contain between 1 and {max} characters.", name);
    }

    private static bool IsSha(string value)
        => System.Text.RegularExpressions.Regex.IsMatch(value, "^[0-9a-fA-F]{40}$");
}

/// <summary>
/// An explicitly attributed assessment of one provider result. These fields are model/evaluator
/// interpretations, not verified CI facts; omitted assessments do not erase failed provider results.
/// </summary>
public sealed record HeadlessAiProviderAssessment
{
    /// <summary>Provider identity matching the referenced result.</summary>
    public required string ProviderId { get; init; }
    /// <summary>Unique turn identifier of the referenced provider result.</summary>
    public required string TurnId { get; init; }
    /// <summary>Provider-attributed findings.</summary>
    public IReadOnlyList<HeadlessAiProviderFinding> Findings { get; init; } = Array.Empty<HeadlessAiProviderFinding>();
    /// <summary>Relevant task or evidence aspects this provider did not address.</summary>
    public IReadOnlyList<string> Omissions { get; init; } = Array.Empty<string>();
    /// <summary>Assessment of evidence quality, with source attribution retained in the text.</summary>
    public IReadOnlyList<string> EvidenceQualityNotes { get; init; } = Array.Empty<string>();
    /// <summary>Provider-reported or evaluator-recorded confidence from zero to one.</summary>
    public double? Confidence { get; init; }
    /// <summary>Uncertainty and limitations associated with this provider's assessment.</summary>
    public string? Uncertainty { get; init; }
    /// <summary>Non-executable next-step suggestions attributed to this provider.</summary>
    public IReadOnlyList<string> ProposedNextActions { get; init; } = Array.Empty<string>();

    /// <summary>Validates bounded, provider-attributed assessment data.</summary>
    public void Validate()
    {
        Require(ProviderId, nameof(ProviderId), 128);
        Require(TurnId, nameof(TurnId), 128);
        if (Confidence is < 0 or > 1 || (Confidence.HasValue && double.IsNaN(Confidence.Value)))
            throw new ArgumentOutOfRangeException(nameof(Confidence));
        ArgumentNullException.ThrowIfNull(Findings);
        ArgumentNullException.ThrowIfNull(Omissions);
        ArgumentNullException.ThrowIfNull(EvidenceQualityNotes);
        ArgumentNullException.ThrowIfNull(ProposedNextActions);
        if (Findings.Count > 128 || Omissions.Count > 128 || EvidenceQualityNotes.Count > 128 || ProposedNextActions.Count > 128)
            throw new ArgumentException("Assessment collections are limited to 128 items.");
        foreach (var finding in Findings)
        {
            ArgumentNullException.ThrowIfNull(finding);
            finding.Validate(ProviderId);
        }
        ValidateTexts(Omissions, nameof(Omissions), 2_048);
        ValidateTexts(EvidenceQualityNotes, nameof(EvidenceQualityNotes), 4_096);
        ValidateTexts(ProposedNextActions, nameof(ProposedNextActions), 2_048);
        if (Uncertainty?.Length > 4_096)
            throw new ArgumentException("Uncertainty exceeds 4096 characters.", nameof(Uncertainty));
    }

    private static void ValidateTexts(IReadOnlyList<string> values, string name, int max)
    {
        if (values.Any(value => string.IsNullOrWhiteSpace(value) || value.Length > max))
            throw new ArgumentException($"{name} entries must be non-empty and limited to {max} characters.", name);
    }

    private static void Require(string value, string name, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max)
            throw new ArgumentException($"{name} must contain between 1 and {max} characters.", name);
    }
}

/// <summary>A single provider-attributed claim or observation; it is not a verified fact.</summary>
public sealed record HeadlessAiProviderFinding
{
    /// <summary>Stable identifier unique within its comparison.</summary>
    public required string FindingId { get; init; }
    /// <summary>Provider that produced this finding.</summary>
    public required string ProviderId { get; init; }
    /// <summary>Untrusted finding text.</summary>
    public required string Statement { get; init; }

    internal void Validate(string expectedProviderId)
    {
        if (string.IsNullOrWhiteSpace(FindingId) || FindingId.Length > 128 ||
            string.IsNullOrWhiteSpace(ProviderId) || ProviderId.Length > 128 ||
            !string.Equals(ProviderId, expectedProviderId, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(Statement) || Statement.Length > 8_192)
            throw new ArgumentException("Finding identity, provider attribution, and bounded statement are required.");
    }
}

/// <summary>
/// An explicitly recorded relationship between findings. Agreement and disagreement are labels
/// supplied by an evaluator/host; HeadlessAi does not derive or resolve them.
/// </summary>
public sealed record HeadlessAiAgentTurnFindingRelationship
{
    /// <summary>Relationship kind: agreement or disagreement.</summary>
    public required string Kind { get; init; }
    /// <summary>At least two finding IDs participating in the relationship.</summary>
    public required IReadOnlyList<string> FindingIds { get; init; }
    /// <summary>Short rationale for the recorded relationship.</summary>
    public required string Explanation { get; init; }

    internal void Validate(IReadOnlyDictionary<string, HeadlessAiProviderFinding> findingsById)
    {
        if (!string.Equals(Kind, "agreement", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Kind, "disagreement", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Relationship kind must be agreement or disagreement.", nameof(Kind));
        ArgumentNullException.ThrowIfNull(FindingIds);
        if (FindingIds.Count < 2 || FindingIds.Count > 32 ||
            FindingIds.Distinct(StringComparer.Ordinal).Count() != FindingIds.Count)
            throw new ArgumentException("A finding relationship requires two to 32 distinct finding IDs.", nameof(FindingIds));
        var providers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in FindingIds)
        {
            if (!findingsById.TryGetValue(id, out var finding))
                throw new ArgumentException($"Relationship references unknown finding '{id}'.", nameof(FindingIds));
            providers.Add(finding.ProviderId);
        }
        if (providers.Count < 2)
            throw new ArgumentException("A relationship must preserve findings from at least two distinct providers.", nameof(FindingIds));
        if (string.IsNullOrWhiteSpace(Explanation) || Explanation.Length > 4_096)
            throw new ArgumentException("Relationship explanation is required and limited to 4096 characters.", nameof(Explanation));
    }
}
