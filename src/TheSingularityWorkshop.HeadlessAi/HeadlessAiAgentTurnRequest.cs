using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// An immutable canonical snapshot of an agent-turn envelope. Create once and share across provider
/// executors to guarantee every provider receives identical serialized input and request fingerprint.
/// </summary>
public sealed class HeadlessAiAgentTurnRequest
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 32
    };

    private readonly HeadlessAiAgentTurnEnvelope _envelope;
    private readonly HeadlessAiAcceptedCiReportContext? _acceptedCiReport;

    /// <summary>The exact serialized envelope shared with every provider.</summary>
    public string CanonicalPayload { get; }

    /// <summary>Lowercase SHA-256 fingerprint of <see cref="CanonicalPayload"/>.</summary>
    public string Fingerprint { get; }

    internal HeadlessAiAgentTurnEnvelope Envelope => _envelope;

    /// <summary>Host-accepted CiAi report context, when this turn is a CI repair turn.</summary>
    public HeadlessAiAcceptedCiReportContext? AcceptedCiReport => _acceptedCiReport;

    private HeadlessAiAgentTurnRequest(
        HeadlessAiAgentTurnEnvelope envelope,
        HeadlessAiAcceptedCiReportContext? acceptedCiReport,
        string canonicalPayload,
        string fingerprint)
    {
        _envelope = envelope;
        _acceptedCiReport = acceptedCiReport;
        CanonicalPayload = canonicalPayload;
        Fingerprint = fingerprint;
    }

    /// <summary>
    /// Validates and snapshots an envelope at one point in time. Subsequent changes to caller-owned
    /// collections or record instances cannot change this request or its fingerprint.
    /// </summary>
    /// <param name="envelope">Host-created turn envelope.</param>
    /// <param name="now">Current time used for expiry and lifetime validation.</param>
    public static HeadlessAiAgentTurnRequest Create(HeadlessAiAgentTurnEnvelope envelope, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        envelope.Validate(now);

        return CreateCore(envelope, null, now);
    }

    /// <summary>Creates a canonical request containing a separately host-accepted, opaque CiAi report.</summary>
    public static HeadlessAiAgentTurnRequest Create(
        HeadlessAiAgentTurnEnvelope envelope,
        HeadlessAiAcceptedCiReportContext acceptedCiReport,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(acceptedCiReport);
        envelope.Validate(now);
        acceptedCiReport.Validate(envelope, now);
        return CreateCore(envelope, acceptedCiReport, now);
    }

    private static HeadlessAiAgentTurnRequest CreateCore(
        HeadlessAiAgentTurnEnvelope envelope,
        HeadlessAiAcceptedCiReportContext? acceptedCiReport,
        DateTimeOffset now)
    {
        var canonicalPayload = acceptedCiReport is null
            ? JsonSerializer.Serialize(envelope, JsonOptions)
            : JsonSerializer.Serialize(new HeadlessAiAgentTurnProviderPayload
            {
                Envelope = envelope,
                AcceptedCiReport = acceptedCiReport
            }, JsonOptions);
        if (canonicalPayload.Length > envelope.Limits.MaxSerializedEnvelopeCharacters)
            throw new ArgumentException("Serialized turn envelope exceeds the configured character limit.", nameof(envelope));

        HeadlessAiAgentTurnEnvelope snapshot;
        HeadlessAiAcceptedCiReportContext? acceptedReportSnapshot = null;
        if (acceptedCiReport is null)
        {
            snapshot = JsonSerializer.Deserialize<HeadlessAiAgentTurnEnvelope>(canonicalPayload, JsonOptions)
                ?? throw new InvalidOperationException("The validated turn envelope could not be snapshotted.");
        }
        else
        {
            var providerPayload = JsonSerializer.Deserialize<HeadlessAiAgentTurnProviderPayload>(canonicalPayload, JsonOptions)
                ?? throw new InvalidOperationException("The accepted-report turn payload could not be snapshotted.");
            snapshot = providerPayload.Envelope;
            acceptedReportSnapshot = providerPayload.AcceptedCiReport;
        }
        snapshot.Validate(now);
        acceptedReportSnapshot?.Validate(snapshot, now);

        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayload)))
            .ToLowerInvariant();
        return new HeadlessAiAgentTurnRequest(snapshot, acceptedReportSnapshot, canonicalPayload, fingerprint);
    }
}


/// <summary>Provider-only composite payload; report content remains opaque to HeadlessAi.</summary>
internal sealed record HeadlessAiAgentTurnProviderPayload
{
    public required HeadlessAiAgentTurnEnvelope Envelope { get; init; }
    public required HeadlessAiAcceptedCiReportContext AcceptedCiReport { get; init; }
}
