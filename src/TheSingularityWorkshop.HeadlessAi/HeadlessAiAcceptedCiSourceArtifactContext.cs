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
    public required string Repository { get; init; }
    public required string WorkflowPath { get; init; }
    public required long RunId { get; init; }
    public required int RunAttempt { get; init; }
    public required string CandidateCommitSha { get; init; }
    public required long JobId { get; init; }
    public required string JobName { get; init; }
    public required string JobKey { get; init; }
    public required long ArtifactId { get; init; }
    public required string ArtifactName { get; init; }
    public required string ProviderArchiveDigest { get; init; }
    public required long ArchiveSizeBytes { get; init; }
    public required DateTimeOffset JobStartedAt { get; init; }
    public required DateTimeOffset JobCompletedAt { get; init; }
    public required DateTimeOffset ArtifactCreatedAt { get; init; }
    public required DateTimeOffset ArtifactExpiresAt { get; init; }
    public required string PayloadSha256 { get; init; }
    public required long PayloadSizeBytes { get; init; }
    public required string IdentitySha256 { get; init; }

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
