using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Nested provider-observed TRX source artifact provenance. The host supplies this only after
/// independently observing the provider run/job/artifact and verifying the bounded archive and
/// extracted payload. HeadlessAi validates the binding and carries it unchanged; it does not
/// retrieve, parse, or re-attest the provider archive.
/// </summary>
public sealed record HeadlessAiAcceptedCiSourceArtifactContext
{
    /// <summary>Repository owner/name identity for the workflow run.</summary>
    public required string Repository { get; init; }
    /// <summary>Repository-relative workflow definition path associated with the run.</summary>
    public required string WorkflowPath { get; init; }
    /// <summary>Provider workflow run identifier.</summary>
    public required long RunId { get; init; }
    /// <summary>Attempt number of the provider workflow run.</summary>
    public required int RunAttempt { get; init; }
    /// <summary>Full commit SHA checked out by the workflow run.</summary>
    public required string CandidateCommitSha { get; init; }
    /// <summary>Provider job identifier for the job that produced the source artifact.</summary>
    public required long JobId { get; init; }
    /// <summary>Display name of the source job.</summary>
    public required string JobName { get; init; }
    /// <summary>Stable machine-readable key assigned to the source job by the host.</summary>
    public required string JobKey { get; init; }
    /// <summary>Provider artifact identifier.</summary>
    public required long ArtifactId { get; init; }
    /// <summary>Name of the provider artifact containing the bounded source archive.</summary>
    public required string ArtifactName { get; init; }
    /// <summary>SHA-256 digest of the exact provider archive bytes, prefixed with sha256:.</summary>
    public required string ProviderArchiveDigest { get; init; }
    /// <summary>Size in bytes of the exact provider archive.</summary>
    public required long ArchiveSizeBytes { get; init; }
    /// <summary>Provider-observed job start time.</summary>
    public required DateTimeOffset JobStartedAt { get; init; }
    /// <summary>Provider-observed job completion time.</summary>
    public required DateTimeOffset JobCompletedAt { get; init; }
    /// <summary>Provider-observed artifact creation time.</summary>
    public required DateTimeOffset ArtifactCreatedAt { get; init; }
    /// <summary>Provider-observed artifact expiration time.</summary>
    public required DateTimeOffset ArtifactExpiresAt { get; init; }
    /// <summary>Lowercase SHA-256 digest of the exact extracted payload bytes.</summary>
    public required string PayloadSha256 { get; init; }
    /// <summary>Size in bytes of the exact extracted payload.</summary>
    public required long PayloadSizeBytes { get; init; }
    /// <summary>Lowercase SHA-256 fingerprint of the canonical provenance fields in this context.</summary>
    public required string IdentitySha256 { get; init; }

    /// <summary>Validates this source-artifact provenance against its accepted report and the current time.</summary>
    /// <param name="report">The outer host-accepted report whose run, attempt, repository, workflow, and candidate lineage must match.</param>
    /// <param name="now">The current time used to reject expired artifacts.</param>
    /// <exception cref="ArgumentNullException"><paramref name="report"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The provenance is malformed, expired, mismatched, or has an invalid identity fingerprint.</exception>
    public void Validate(HeadlessAiAcceptedCiReportContext report, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(report);
        static bool Clean(string? value, int maximum) =>
            !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && !value.Any(char.IsControl);
        static bool IsSha(string? value) => value is { Length: 40 } && value.All(Uri.IsHexDigit);
        static bool IsDigest(string? value) => value is { Length: 64 } && value.All(Uri.IsHexDigit);

        var repositoryParts = Repository?.Split('/') ?? Array.Empty<string>();
        if (!Clean(Repository, 200) || repositoryParts.Length != 2 ||
            repositoryParts.Any(part => !Clean(part, 100)) ||
            !Clean(WorkflowPath, 512) || !WorkflowPath.StartsWith(".github/workflows/", StringComparison.Ordinal) ||
            RunId <= 0 || RunAttempt <= 0 || !IsSha(CandidateCommitSha) ||
            JobId <= 0 || !Clean(JobName, 200) || !Clean(JobKey, 100) ||
            JobKey.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.')) ||
            ArtifactId <= 0 || !Clean(ArtifactName, 200) ||
            ProviderArchiveDigest is not { Length: 71 } ||
            !ProviderArchiveDigest.StartsWith("sha256:", StringComparison.Ordinal) ||
            !ProviderArchiveDigest.AsSpan(7).ToString().All(Uri.IsHexDigit) ||
            ArchiveSizeBytes is <= 0 or > 20 * 1024 * 1024 ||
            !IsDigest(PayloadSha256) || PayloadSizeBytes is <= 0 or > 16 * 1024 * 1024 ||
            JobStartedAt == default || JobCompletedAt < JobStartedAt ||
            JobCompletedAt > report.ReportObservedAt ||
            ArtifactCreatedAt < JobStartedAt || ArtifactCreatedAt > JobCompletedAt ||
            ArtifactCreatedAt > report.ReportObservedAt ||
            ArtifactExpiresAt <= ArtifactCreatedAt || ArtifactExpiresAt <= report.ReportObservedAt ||
            ArtifactExpiresAt <= now ||
            IdentitySha256 is not { Length: 64 } || !IdentitySha256.All(Uri.IsHexDigit))
            throw new ArgumentException("Accepted nested source-artifact provenance is malformed, expired, or outside the observed job-attempt window.");

        if (!string.Equals(Repository, report.Repository, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(WorkflowPath, report.WorkflowPath, StringComparison.Ordinal) ||
            !long.TryParse(report.WorkflowRunId, NumberStyles.None, CultureInfo.InvariantCulture, out var reportRunId) ||
            RunId != reportRunId || RunAttempt != report.RunAttempt ||
            !string.Equals(CandidateCommitSha, report.CandidateCommitSha, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Nested source-artifact provenance does not match the outer accepted report lineage.");

        var expectedIdentity = ComputeIdentitySha256(this);
        if (!string.Equals(IdentitySha256, expectedIdentity, StringComparison.Ordinal))
            throw new ArgumentException("Nested source-artifact identity fingerprint does not match its exact fields.", nameof(IdentitySha256));
    }

    /// <summary>Computes the canonical SHA-256 identity fingerprint for the supplied source-artifact fields.</summary>
    /// <param name="source">The source-artifact context to fingerprint.</param>
    /// <returns>The lowercase hexadecimal SHA-256 digest of its canonical provenance fields.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static string ComputeIdentitySha256(HeadlessAiAcceptedCiSourceArtifactContext source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var material = string.Join("\n", new[]
        {
            source.Repository.ToLowerInvariant(),
            source.RunId.ToString(CultureInfo.InvariantCulture),
            source.RunAttempt.ToString(CultureInfo.InvariantCulture),
            source.CandidateCommitSha.ToLowerInvariant(),
            source.WorkflowPath,
            source.JobId.ToString(CultureInfo.InvariantCulture),
            source.JobName,
            source.JobKey,
            source.ArtifactId.ToString(CultureInfo.InvariantCulture),
            source.ArtifactName,
            source.ProviderArchiveDigest.ToLowerInvariant(),
            source.ArchiveSizeBytes.ToString(CultureInfo.InvariantCulture),
            source.JobStartedAt.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture),
            source.JobCompletedAt.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture),
            source.ArtifactCreatedAt.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture),
            source.ArtifactExpiresAt.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture),
            source.PayloadSha256.ToLowerInvariant(),
            source.PayloadSizeBytes.ToString(CultureInfo.InvariantCulture)
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material))).ToLowerInvariant();
    }
}
