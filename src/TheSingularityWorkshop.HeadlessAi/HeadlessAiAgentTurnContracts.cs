using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// A self-describing, versioned model turn. This is a data contract only: it does not authorize
/// tools, repository writes, workflow transitions, or any other operation.
/// </summary>
public sealed record HeadlessAiAgentTurnEnvelope
{
    /// <summary>Current supported turn envelope schema version.</summary>
    public const int CurrentVersion = 1;

    /// <summary>Stable task identifier owned by the host.</summary>
    public required string TaskId { get; init; }
    /// <summary>Correlation identifier shared by all exchanges in one task.</summary>
    public required string CorrelationId { get; init; }
    /// <summary>Repository identity in owner/name form.</summary>
    public required string Repository { get; init; }
    /// <summary>Feature branch authorized by the host for the candidate.</summary>
    public required string AuthorizedBranch { get; init; }
    /// <summary>Full commit SHA from which the authorized branch was based.</summary>
    public required string BaseCommitSha { get; init; }
    /// <summary>Current candidate SHA, when a candidate already exists.</summary>
    public string? CandidateCommitSha { get; init; }
    /// <summary>Optional issue URL associated with the task.</summary>
    public string? IssueUrl { get; init; }
    /// <summary>Optional pull request URL associated with the task.</summary>
    public string? PullRequestUrl { get; init; }
    /// <summary>Requested model role label; it does not grant authority.</summary>
    public required string Role { get; init; }
    /// <summary>Task instructions supplied by the host.</summary>
    public required string Instructions { get; init; }
    /// <summary>Canonical prompt shared by providers in a comparison.</summary>
    public required string CanonicalPrompt { get; init; }
    /// <summary>Optional expected response schema description.</summary>
    public string? ResponseSchema { get; init; }
    /// <summary>Absolute expiry after which the host must reject this turn.</summary>
    public required DateTimeOffset ExpiresAt { get; init; }
    /// <summary>Prior turn identifier for a resumable exchange.</summary>
    public string? PriorTurnId { get; init; }
    /// <summary>Prior candidate SHA whose CI result is being continued.</summary>
    public string? PriorCandidateCommitSha { get; init; }
    /// <summary>Host-generated idempotency key for duplicate-delivery protection.</summary>
    public string? IdempotencyKey { get; init; }
    /// <summary>Envelope schema version.</summary>
    public int Version { get; init; } = CurrentVersion;
    /// <summary>Resource, retry, concurrency, and timeout bounds.</summary>
    public HeadlessAiAgentTurnLimits Limits { get; init; } = new();
    /// <summary>Untrusted CI evidence references with preserved provenance.</summary>
    public IReadOnlyList<HeadlessAiEvidenceReference> Evidence { get; init; } = Array.Empty<HeadlessAiEvidenceReference>();
    /// <summary>Bounded external context references; content retrieval remains host-owned.</summary>
    public IReadOnlyList<string> ContextReferences { get; init; } = Array.Empty<string>();

    /// <summary>Validates structural bounds and provenance shape before an envelope is sent.</summary>
    public void Validate(DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(Limits);
        ArgumentNullException.ThrowIfNull(Evidence);
        ArgumentNullException.ThrowIfNull(ContextReferences);
        Limits.Validate();
        if (Version != CurrentVersion)
            throw new ArgumentOutOfRangeException(nameof(Version), $"Unsupported turn envelope version: {Version}.");
        Require(TaskId, nameof(TaskId), 128);
        Require(CorrelationId, nameof(CorrelationId), 128);
        Require(Repository, nameof(Repository), 256);
        if (!Repository.Contains('/') || Repository.StartsWith('/') || Repository.EndsWith('/'))
            throw new ArgumentException("Repository must use owner/name form.", nameof(Repository));
        Require(AuthorizedBranch, nameof(AuthorizedBranch), 256);
        if (string.Equals(AuthorizedBranch, "master", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(AuthorizedBranch, "development", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Agent turns must target an isolated feature branch, never master or development.", nameof(AuthorizedBranch));
        ValidateSha(BaseCommitSha, nameof(BaseCommitSha));
        if (CandidateCommitSha is not null) ValidateSha(CandidateCommitSha, nameof(CandidateCommitSha));
        if (PriorCandidateCommitSha is not null) ValidateSha(PriorCandidateCommitSha, nameof(PriorCandidateCommitSha));
        Require(Role, nameof(Role), 128);
        Require(Instructions, nameof(Instructions), Limits.MaxInstructionCharacters);
        Require(CanonicalPrompt, nameof(CanonicalPrompt), Limits.MaxPromptCharacters);
        RejectCredentialLikeText(Instructions, nameof(Instructions));
        RejectCredentialLikeText(CanonicalPrompt, nameof(CanonicalPrompt));
        if (ResponseSchema is not null) RejectCredentialLikeText(ResponseSchema, nameof(ResponseSchema));
        if (ExpiresAt <= now) throw new ArgumentException("The turn envelope has expired.", nameof(ExpiresAt));
        if (ExpiresAt - now > Limits.MaxLifetime)
            throw new ArgumentException("The turn expiry exceeds the configured maximum lifetime.", nameof(ExpiresAt));
        if (Evidence.Count > Limits.MaxEvidenceItems)
            throw new ArgumentException("Evidence item count exceeds the configured limit.", nameof(Evidence));
        if (ContextReferences.Count > Limits.MaxContextReferences)
            throw new ArgumentException("Context reference count exceeds the configured limit.", nameof(ContextReferences));
        foreach (var item in Evidence)
        {
            ArgumentNullException.ThrowIfNull(item);
            item.Validate(Repository);
        }
        foreach (var reference in ContextReferences)
        {
            Require(reference, nameof(ContextReferences), Limits.MaxReferenceCharacters);
            RejectCredentialLikeText(reference, nameof(ContextReferences));
        }
        if (IssueUrl is not null) ValidateHttpsUrl(IssueUrl, nameof(IssueUrl));
        if (PullRequestUrl is not null) ValidateHttpsUrl(PullRequestUrl, nameof(PullRequestUrl));
        foreach (var item in Evidence)
            if (item.BoundedSummary is not null) RejectCredentialLikeText(item.BoundedSummary, nameof(Evidence));
        if (PriorTurnId is not null) Require(PriorTurnId, nameof(PriorTurnId), 128);
        if (IdempotencyKey is not null) Require(IdempotencyKey, nameof(IdempotencyKey), 256);
    }

    private static void Require(string value, string name, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max)
            throw new ArgumentException($"{name} must contain between 1 and {max} characters.", name);
    }

    private static void RejectCredentialLikeText(string value, string name)
    {
        string[] patterns =
        [
            @"(?i)\\bBearer\\s+\\S{12,}",
            @"(?i)\\b(?:api[_-]?key|access[_-]?token|client[_-]?secret|password)\\s*[:=]\\s*['"" ]?[^\\s,'"" ]{8,}",
            @"\\b(?:gh[pousr]_[A-Za-z0-9]{20,}|sk-[A-Za-z0-9]{20,})\\b"
        ];
        if (patterns.Any(pattern => Regex.IsMatch(value, pattern, RegexOptions.CultureInvariant)))
            throw new ArgumentException("Prompt and evidence payloads must not contain credentials; use the host-managed header provider.", name);
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
    /// <summary>Maximum instruction text length.</summary>
    public int MaxInstructionCharacters { get; init; } = 16_384;
    /// <summary>Maximum canonical prompt length.</summary>
    public int MaxPromptCharacters { get; init; } = 64_000;
    /// <summary>Maximum number of evidence references.</summary>
    public int MaxEvidenceItems { get; init; } = 64;
    /// <summary>Maximum number of context references.</summary>
    public int MaxContextReferences { get; init; } = 128;
    /// <summary>Maximum length of each context reference.</summary>
    public int MaxReferenceCharacters { get; init; } = 2_048;
    /// <summary>Maximum serialized envelope length before transport.</summary>
    public int MaxSerializedEnvelopeCharacters { get; init; } = 256_000;
    /// <summary>Maximum allowed envelope lifetime from validation time.</summary>
    public TimeSpan MaxLifetime { get; init; } = TimeSpan.FromHours(2);
    /// <summary>Per-provider invocation timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(2);
    /// <summary>Maximum provider attempts permitted by the host policy.</summary>
    public int MaxProviderAttempts { get; init; } = 1;
    /// <summary>Maximum provider fan-out concurrency permitted by the host policy.</summary>
    public int MaxConcurrentProviders { get; init; } = 4;

    internal void Validate()
    {
        if (MaxInstructionCharacters <= 0 || MaxPromptCharacters <= 0 || MaxEvidenceItems < 0 ||
            MaxContextReferences < 0 || MaxReferenceCharacters <= 0 || MaxSerializedEnvelopeCharacters <= 0 || MaxLifetime <= TimeSpan.Zero ||
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
    /// <summary>Repository identity reported by the CI evidence source.</summary>
    public required string Repository { get; init; }
    /// <summary>Full SHA to which this evidence applies.</summary>
    public required string CommitSha { get; init; }
    /// <summary>CI provider's workflow run identifier.</summary>
    public required string WorkflowRunId { get; init; }
    /// <summary>Workflow run attempt number.</summary>
    public required int Attempt { get; init; }
    /// <summary>Workflow display name.</summary>
    public required string WorkflowName { get; init; }
    /// <summary>Optional job name within the workflow.</summary>
    public string? JobName { get; init; }
    /// <summary>Optional step name within the job.</summary>
    public string? StepName { get; init; }
    /// <summary>CI-reported conclusion, preserved as untrusted source data.</summary>
    public required string Conclusion { get; init; }
    /// <summary>HTTPS URL of the source evidence.</summary>
    public required string SourceUrl { get; init; }
    /// <summary>Time at which the host observed this evidence.</summary>
    public required DateTimeOffset ObservedAt { get; init; }
    /// <summary>Optional bounded evidence excerpt; may contain untrusted text.</summary>
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
    /// <summary>Model-proposed diagnosis; not a verified fact.</summary>
    public string? Diagnosis { get; init; }
    /// <summary>Model-proposed patch intent; not an executable patch.</summary>
    public string? PatchIntent { get; init; }
    /// <summary>Questions the model proposes asking the host or owner.</summary>
    public IReadOnlyList<string> Questions { get; init; } = Array.Empty<string>();
    /// <summary>Model-reported confidence in the proposal, from zero to one.</summary>
    public double? Confidence { get; init; }
    /// <summary>Model-reported uncertainty and missing information.</summary>
    public string? Uncertainty { get; init; }
    /// <summary>Reason to stop or request human intervention.</summary>
    public string? StopOrEscalateReason { get; init; }
    /// <summary>Suggested operational status for the host to evaluate.</summary>
    public string? OperationalStatus { get; init; }
    /// <summary>Human decision or authorization requested by the model.</summary>
    public string? HumanDecisionRequired { get; init; }

    /// <summary>Validates proposal bounds without treating any field as trusted or executable.</summary>
    /// <summary>Validates proposal bounds without treating any field as trusted or executable.</summary>
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Questions);
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
    /// <summary>Unique identifier for this exchange turn.</summary>
    public required string TurnId { get; init; }
    /// <summary>Host-owned task identifier.</summary>
    public required string TaskId { get; init; }
    /// <summary>Correlation identifier shared by resumable turns.</summary>
    public required string CorrelationId { get; init; }
    /// <summary>Provider adapter identity.</summary>
    public required string ProviderId { get; init; }
    /// <summary>Provider model identifier.</summary>
    public required string ModelId { get; init; }
    /// <summary>Optional provider or API version.</summary>
    public string? ProviderVersion { get; init; }
    /// <summary>Stable fingerprint of the canonical request package.</summary>
    public required string RequestFingerprint { get; init; }
    /// <summary>Invocation start timestamp.</summary>
    public required DateTimeOffset StartedAt { get; init; }
    /// <summary>Invocation completion timestamp.</summary>
    public required DateTimeOffset CompletedAt { get; init; }
    /// <summary>Original raw provider response, retained separately from normalized fields.</summary>
    public required string RawResponse { get; init; }
    /// <summary>Normalized, untrusted proposal derived from the raw response.</summary>
    public required HeadlessAiAgentTurnProposal Proposal { get; init; }
    /// <summary>Provider-reported metadata preserved without reinterpretation.</summary>
    public IReadOnlyDictionary<string, string> ProviderMetadata { get; init; } =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    /// <summary>Provider-reported finish or stop reason.</summary>
    public string? FinishReason { get; init; }
    /// <summary>Optional diagnostic text for the exchange.</summary>
    public string? Diagnostic { get; init; }
    /// <summary>Optional provider-neutral error code.</summary>
    public string? ErrorCode { get; init; }
    /// <summary>Provider-reported input token count, when available.</summary>
    public long? InputTokens { get; init; }
    /// <summary>Provider-reported output token count, when available.</summary>
    public long? OutputTokens { get; init; }

    /// <summary>Validates exchange identity, timing, metadata bounds, and normalized proposal.</summary>
    /// <summary>Validates exchange identity, timing, metadata bounds, and normalized proposal.</summary>
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(ProviderMetadata);
        ArgumentNullException.ThrowIfNull(Proposal);
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
