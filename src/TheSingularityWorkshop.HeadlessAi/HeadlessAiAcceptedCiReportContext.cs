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
    private const int MaximumReportBytes = 1_048_576;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// <summary>Stable identity assigned to the accepted canonical report by its owner.</summary>
    public required string ReportIdentity { get; init; }

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
    public void Validate(HeadlessAiAgentTurnEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        if (string.IsNullOrWhiteSpace(ReportIdentity) || ReportIdentity.Length > 256)
            throw new ArgumentException("Accepted report identity is required and limited to 256 characters.", nameof(ReportIdentity));
        if (string.IsNullOrWhiteSpace(WorkflowRunId) || WorkflowRunId.Length > 128)
            throw new ArgumentException("Accepted report workflow run identity is required and bounded.", nameof(WorkflowRunId));
        if (RunAttempt < 1)
            throw new ArgumentOutOfRangeException(nameof(RunAttempt));
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

        if (!Regex.IsMatch(ReportSha256 ?? string.Empty, "^[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Accepted report digest must be a full SHA-256 value.", nameof(ReportSha256));

        var actualDigest = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (!string.Equals(actualDigest, ReportSha256, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Accepted report digest does not match its exact UTF-8 bytes.", nameof(ReportSha256));
    }
}
