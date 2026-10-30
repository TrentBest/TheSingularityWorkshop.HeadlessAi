using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// A self-describing, versioned model turn. This is a data contract only: it does not authorize
/// tools, repository writes, workflow transitions, or any other operation.
/// </summary>
public sealed record HeadlessAiAgentTurnEnvelope
{
    public const int CurrentVersion = 1;

    public required string TaskId { get; init; }
    public required string CorrelationId { get; init; }
    public required string Repository { get; init; }
    public required string AuthorizedBranch { get; init; }
    public required string BaseCommitSha { get; init; }
    public string? CandidateCommitSha { get; init; }
    public string? IssueUrl { get; init; }
    public string? PullRequestUrl { get; init; }
    public required string Role { get; init; }
    public required string Instructions { get; init; }
    public required string CanonicalPrompt { get; init; }
    public string? ResponseSchema { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public string? PriorTurnId { get; init; }
    public string? PriorCandidateCommitSha { get; init; }
    public string? IdempotencyKey { get; init; }
    public int Version { get; init; } = CurrentVersion;
    public HeadlessAiAgentTurnLimits Limits { get; init; } = new();
    public IReadOnlyList<HeadlessAiEvidenceReference> Evidence { get; init; } = Array.Empty<HeadlessAiEvidenceReference>();
    public IReadOnlyList<string> ContextReferences { get; init; } = Array.Empty<string>();

    /// <summary>Validates structural bounds and provenance shape before an envelope is sent.</summary>
    public void Validate(DateTimeOffset now)
    {
        if (Version != CurrentVersion)
            throw new ArgumentOutOfRangeException(nameof(Version), $"Unsupported turn envelope version: {Version}.");
        Require(TaskId, nameof(TaskId), 128);
        Require(CorrelationId, nameof(CorrelationId), 128);
        Require(Repository, nameof(Repository), 256);
        if (!Repository.Contains('/') || Repository.StartsWith('/') || Repository.EndsWith('/'))
            throw new ArgumentException("Repository must use owner/name form.", nameof(Repository));
        Require(AuthorizedBranch, nameof(AuthorizedBranch), 256);
        ValidateSha(BaseCommitSha, nameof(BaseCommitSha));
        if (CandidateCommitSha is not null) ValidateSha(CandidateCommitSha, nameof(CandidateCommitSha));
        if (PriorCandidateCommitSha is not null) ValidateSha(PriorCandidateCommitSha, nameof(PriorCandidateCommitSha));
        Require(Role, nameof(Role), 128);
        Require(Instructions, nameof(Instructions), Limits.MaxInstructionCharacters);
        Require(CanonicalPrompt, nameof(CanonicalPrompt), Limits.MaxPromptCharacters);
        if (ExpiresAt <= now) throw new ArgumentException("The turn envelope has expired.", nameof(ExpiresAt));
        if (ExpiresAt - now > Limits.MaxLifetime)
            throw new ArgumentException("The turn expiry exceeds the configured maximum lifetime.", nameof(ExpiresAt));
        if (Evidence.Count > Limits.MaxEvidenceItems)
            throw new ArgumentException("Evidence item count exceeds the configured limit.", nameof(Evidence));
        if (ContextReferences.Count > Limits.MaxContextReferences)
            throw new ArgumentException("Context reference count exceeds the configured limit.", nameof(ContextReferences));
        foreach (var item in Evidence) item.Validate(Repository);
        foreach (var reference in ContextReferences) Require(reference, nameof(ContextReferences), Limits.MaxReferenceCharacters);
        if (IssueUrl is not null) ValidateHttpsUrl(IssueUrl, nameof(IssueUrl));
        if (PullRequestUrl is not null) ValidateHttpsUrl(PullRequestUrl, nameof(PullRequestUrl));
        if (PriorTurnId is not null) Require(PriorTurnId, nameof(PriorTurnId), 128);
        if (IdempotencyKey is not null) Require(IdempotencyKey, nameof(IdempotencyKey), 256);
        Limits.Validate();
    }

    private static void Require(string value, string name, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max)
            throw new ArgumentException($"{name} must contain between 1 and {max} characters.", name);
    }

    private static void ValidateSha(string sha, string name)
    {
        if (sha is null || !Regex.IsMatch(sha, "^[0-9a-fA-F]{40}$", RegexOptions.CultureInvariant))
            throw new ArgumentException($"{name} must be a full 40-character commit SHA.", name);
    }

    private static void ValidateHttpsUrl(string value, string name)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException($"{name} must be an absolute HTTPS URL.", name);
    }
}

/// <summary>Explicit resource and time bounds for a single model turn.</summary>
public sealed record HeadlessAiAgentTurnLimits
{
    public int MaxInstructionCharacters { get; init; } = 16_384;
    public int MaxPromptCharacters { get; init; } = 64_000;
    public int MaxEvidenceItems { get; init; } = 64;
    public int MaxContextReferences { get; init; } = 128;
    public int MaxReferenceCharacters { get; init; } = 2_048;
    public TimeSpan MaxLifetime { get; init; } = TimeSpan.FromHours(2);
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(2);
    public int MaxProviderAttempts { get; init; } = 1;
    public int MaxConcurrentProviders { get; init; } = 4;

    internal void Validate()
    {
        if (MaxInstructionCharacters <= 0 || MaxPromptCharacters <= 0 || MaxEvidenceItems < 0 ||
            MaxContextReferences < 0 || MaxReferenceCharacters <= 0 || MaxLifetime <= TimeSpan.Zero ||
            Timeout <= TimeSpan.Zero || MaxProviderAttempts < 1 || MaxConcurrentProviders < 1)
            throw new ArgumentOutOfRangeException(nameof(HeadlessAiAgentTurnLimits), "Turn limits must be positive (counts may be zero where documented).");
    }
}

/// <summary>
/// A provenance-bearing reference to CI evidence. Evidence content is untrusted input and is never
/// interpreted as authorization or as a verified CI verdict by HeadlessAi.
/// </summary>
public sealed record HeadlessAiEvidenceReference
{
    public required string Repository { get; init; }
    public required string CommitSha { get; init; }
    public required string WorkflowRunId { get; init; }
    public required int Attempt { get; init; }
    public required string WorkflowName { get; init; }
    public string? JobName { get; init; }
    public string? StepName { get; init; }
    public required string Conclusion { get; init; }
    public required string SourceUrl { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public string? BoundedSummary { get; init; }

    internal void Validate(string expectedRepository)
    {
        if (!string.Equals(Repository, expectedRepository, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Evidence repository does not match the turn repository.", nameof(Repository));
        if (!Regex.IsMatch(CommitSha ?? string.Empty, "^[0-9a-fA-F]{40}$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Evidence must identify a full commit SHA.", nameof(CommitSha));
        if (string.IsNullOrWhiteSpace(WorkflowRunId) || WorkflowRunId.Length > 128)
            throw new ArgumentException("Workflow run ID is required and bounded.", nameof(WorkflowRunId));
        if (Attempt < 1) throw new ArgumentOutOfRangeException(nameof(Attempt));
        if (string.IsNullOrWhiteSpace(WorkflowName) || WorkflowName.Length > 256)
            throw new ArgumentException("Workflow name is required and bounded.", nameof(WorkflowName));
        if (string.IsNullOrWhiteSpace(Conclusion) || Conclusion.Length > 64)
            throw new ArgumentException("Conclusion is required and bounded.", nameof(Conclusion));
        if (!Uri.TryCreate(SourceUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("Evidence source must be an absolute HTTPS URL.", nameof(SourceUrl));
        if (BoundedSummary?.Length > 16_384)
            throw new ArgumentException("Evidence summary exceeds 16384 characters.", nameof(BoundedSummary));
    }
}

/// <summary>
/// A normalized model proposal. Every field remains untrusted data; consumers must independently
/// validate it and apply host-owned authorization before taking any action.
/// </summary>
public sealed record HeadlessAiAgentTurnProposal
{
    public string? Diagnosis { get; init; }
    public string? PatchIntent { get; init; }
    public IReadOnlyList<string> Questions { get; init; } = Array.Empty<string>();
    public double? Confidence { get; init; }
    public string? Uncertainty { get; init; }
    public string? StopOrEscalateReason { get; init; }
    public string? OperationalStatus { get; init; }
    public string? HumanDecisionRequired { get; init; }

    public void Validate()
    {
        if (Confidence is < 0 or > 1 || (Confidence.HasValue && double.IsNaN(Confidence.Value)))
            throw new ArgumentOutOfRangeException(nameof(Confidence), "Confidence must be between zero and one.");
        if (Questions.Count > 64 || Questions.Any(q => string.IsNullOrWhiteSpace(q) || q.Length > 2_048))
            throw new ArgumentException("Questions must be non-empty and limited to 2048 characters each (maximum 64).", nameof(Questions));
        Check(Diagnosis, 16_384, nameof(Diagnosis));
        Check(PatchIntent, 16_384, nameof(PatchIntent));
        Check(Uncertainty, 4_096, nameof(Uncertainty));
        Check(StopOrEscalateReason, 4_096, nameof(StopOrEscalateReason));
        Check(OperationalStatus, 2_048, nameof(OperationalStatus));
        Check(HumanDecisionRequired, 4_096, nameof(HumanDecisionRequired));
    }

    private static void Check(string? value, int max, string name)
    {
        if (value?.Length > max) throw new ArgumentException($"{name} exceeds {max} characters.", name);
    }
}

/// <summary>
/// A completed exchange with explicit provenance. Raw provider output and provider facts remain
/// separate from the normalized proposal; neither is an executable instruction.
/// </summary>
public sealed record HeadlessAiAgentTurnResult
{
    public required string TurnId { get; init; }
    public required string TaskId { get; init; }
    public required string CorrelationId { get; init; }
    public required string ProviderId { get; init; }
    public required string ModelId { get; init; }
    public string? ProviderVersion { get; init; }
    public required string RequestFingerprint { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
    public required string RawResponse { get; init; }
    public required HeadlessAiAgentTurnProposal Proposal { get; init; }
    public IReadOnlyDictionary<string, string> ProviderMetadata { get; init; } =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    public string? FinishReason { get; init; }
    public string? Diagnostic { get; init; }
    public string? ErrorCode { get; init; }
    public long? InputTokens { get; init; }
    public long? OutputTokens { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TurnId) || string.IsNullOrWhiteSpace(TaskId) ||
            string.IsNullOrWhiteSpace(CorrelationId) || string.IsNullOrWhiteSpace(ProviderId) ||
            string.IsNullOrWhiteSpace(ModelId) || string.IsNullOrWhiteSpace(RequestFingerprint))
            throw new ArgumentException("Turn identity, provider, model, and request fingerprint are required.");
        if (CompletedAt < StartedAt) throw new ArgumentException("Completion cannot precede start.");
        if (InputTokens is < 0 || OutputTokens is < 0) throw new ArgumentOutOfRangeException(nameof(InputTokens));
        if (RawResponse is null || RawResponse.Length > 1_000_000)
            throw new ArgumentException("Raw provider response is required and limited to one million characters.", nameof(RawResponse));
        if (ProviderMetadata.Count > 128 || ProviderMetadata.Any(p => p.Key.Length > 128 || p.Value.Length > 4_096))
            throw new ArgumentException("Provider metadata exceeds the allowed bounds.", nameof(ProviderMetadata));
        Proposal.Validate();
    }
}
