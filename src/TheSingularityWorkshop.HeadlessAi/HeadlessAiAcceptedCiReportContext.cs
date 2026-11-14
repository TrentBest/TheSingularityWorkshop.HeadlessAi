using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Opaque UTF-8 bytes for a CiAi report that the host has already authenticated and accepted.
/// HeadlessAi verifies only the supplied digest and task/candidate binding; it never parses or
/// re-evaluates the report. The host remains responsible for proving acceptance and trusted lineage.
/// </summary>
public sealed record HeadlessAiAcceptedCiReportContext
{
    /// <summary>The only report schema currently accepted by this handoff contract.</summary>
    public const string RequiredSchemaVersion = "ciai.test-results.v1";
    private const int MaximumReportBytes = 1_048_576;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// <summary>Canonical report schema identifier; CiAi owns this versioned report surface.</summary>
    public string SchemaVersion { get; init; } = RequiredSchemaVersion;

    /// <summary>Verdict parsed and accepted by the host; HeadlessAi does not re-evaluate it.</summary>
    public required string Verdict { get; init; }

    /// <summary>Repository owner/name identity from the host-accepted report mapping.</summary>
    public required string Repository { get; init; }

    /// <summary>Workflow definition path bound by the host to the exact run.</summary>
    public required string WorkflowPath { get; init; }

    /// <summary>Time the host accepted the canonical report.</summary>
    public required DateTimeOffset ReportObservedAt { get; init; }

    /// <summary>Independent provider-observed nested TRX source artifact provenance.</summary>
    public HeadlessAiAcceptedCiSourceArtifactContext? SourceArtifact { get; init; }

    /// <summary>Stable identity assigned to the accepted canonical report by its owner.</summary>
    public required string ReportIdentity { get; init; }
    /// <summary>Fingerprint of the authenticated outer notice, durable receipt, and report artifact identity.</summary>
    public required string NoticeIdentitySha256 { get; init; }

    /// <summary>Exact canonical report JSON, represented as its original UTF-8 text.</summary>
    public required string CanonicalReportJson { get; init; }

    /// <summary>Lowercase SHA-256 digest of the exact UTF-8 report bytes.</summary>
    public required string ReportSha256 { get; init; }

    /// <summary>Workflow run identity supplied by the trusted host acceptance record.</summary>
    public required string WorkflowRunId { get; init; }

    /// <summary>Workflow run attempt supplied by the trusted host acceptance record.</summary>
    public required int RunAttempt { get; init; }

    /// <summary>Candidate checkout SHA to which the accepted report and host record are bound.</summary>
    public required string CandidateCommitSha { get; init; }

    /// <summary>Validates bounds, exact-byte digest, and binding to the host-created turn envelope.</summary>
    public void Validate(HeadlessAiAgentTurnEnvelope envelope, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        if (!string.Equals(SchemaVersion, RequiredSchemaVersion, StringComparison.Ordinal))
            throw new ArgumentException("Only the canonical ciai.test-results.v1 report is accepted.", nameof(SchemaVersion));
        if (string.IsNullOrWhiteSpace(ReportIdentity) || ReportIdentity.Length > 256)
            throw new ArgumentException("Accepted report identity is required and limited to 256 characters.", nameof(ReportIdentity));
        if (NoticeIdentitySha256 is not { Length: 64 } || !NoticeIdentitySha256.All(Uri.IsHexDigit) ||
            !string.Equals(NoticeIdentitySha256, NoticeIdentitySha256.ToLowerInvariant(), StringComparison.Ordinal))
            throw new ArgumentException("Accepted outer notice identity must be a lowercase SHA-256 fingerprint.", nameof(NoticeIdentitySha256));
        if (string.IsNullOrWhiteSpace(WorkflowRunId) || WorkflowRunId.Length > 128)
            throw new ArgumentException("Accepted report workflow run identity is required and bounded.", nameof(WorkflowRunId));
        if (RunAttempt < 1)
            throw new ArgumentOutOfRangeException(nameof(RunAttempt));
        if (!long.TryParse(WorkflowRunId, NumberStyles.None, CultureInfo.InvariantCulture, out var runId) || runId <= 0)
            throw new ArgumentException("Accepted report workflow run identity must be a positive numeric run ID.", nameof(WorkflowRunId));
        if (string.IsNullOrWhiteSpace(Repository) || Repository.Length > 200 ||
            Repository.Split('/').Length != 2 || Repository.Split('/').Any(part =>
                string.IsNullOrWhiteSpace(part) || part.Length > 100 || part.Any(char.IsControl)))
            throw new ArgumentException("Accepted report repository must be in owner/name form.", nameof(Repository));
        if (string.IsNullOrWhiteSpace(WorkflowPath) || WorkflowPath.Length > 512 ||
            !WorkflowPath.StartsWith(".github/workflows/", StringComparison.Ordinal) ||
            WorkflowPath.Any(char.IsControl))
            throw new ArgumentException("Accepted report workflow path is invalid.", nameof(WorkflowPath));
        if (ReportObservedAt == default || ReportObservedAt > now)
            throw new ArgumentException("Accepted report observation time must be present and not in the future.", nameof(ReportObservedAt));
        if (Verdict is not ("CompletePass" or "ActionableFailure" or "NonActionableFailure" or "IncompleteOrUntrusted"))
            throw new ArgumentException("Accepted report verdict is unknown.", nameof(Verdict));
        if ((Verdict is "CompletePass" or "ActionableFailure" or "NonActionableFailure") && SourceArtifact is null)
            throw new ArgumentException("Complete accepted CI evidence requires independently observed source-artifact provenance.", nameof(SourceArtifact));
        SourceArtifact?.Validate(this, now);
        if (!Regex.IsMatch(CandidateCommitSha ?? string.Empty, "^[0-9a-fA-F]{40}$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Accepted report candidate must be a full commit SHA.", nameof(CandidateCommitSha));

        var candidateMatches = string.Equals(CandidateCommitSha, envelope.CandidateCommitSha, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(CandidateCommitSha, envelope.PriorCandidateCommitSha, StringComparison.OrdinalIgnoreCase);
        if (!candidateMatches)
            throw new ArgumentException("Accepted report candidate does not match the current or prior turn candidate.", nameof(CandidateCommitSha));

        if (string.IsNullOrEmpty(CanonicalReportJson))
            throw new ArgumentException("Accepted report bytes are required.", nameof(CanonicalReportJson));

        byte[] bytes;
        try
        {
            bytes = StrictUtf8.GetBytes(CanonicalReportJson);
        }
        catch (EncoderFallbackException ex)
        {
            throw new ArgumentException("Accepted report must contain valid UTF-8 text.", nameof(CanonicalReportJson), ex);
        }

        if (bytes.Length > MaximumReportBytes)
            throw new ArgumentException("Accepted report exceeds the one-megabyte byte limit.", nameof(CanonicalReportJson));
        HeadlessAiAgentTurnEnvelope.RejectCredentialLikeText(CanonicalReportJson, nameof(CanonicalReportJson));

        if (!Regex.IsMatch(ReportSha256 ?? string.Empty, "^[0-9a-f]{64}$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Accepted report digest must be a full SHA-256 value.", nameof(ReportSha256));

        var actualDigest = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (!string.Equals(actualDigest, ReportSha256, StringComparison.Ordinal))
            throw new ArgumentException("Accepted report digest does not match its exact UTF-8 bytes.", nameof(ReportSha256));

        var expectedIdentity = "sha256:" + actualDigest;
        if (!string.Equals(ReportIdentity, expectedIdentity, StringComparison.Ordinal))
            throw new ArgumentException("Accepted report identity must be sha256: followed by the lowercase digest of the exact UTF-8 bytes.", nameof(ReportIdentity));
    }
}
