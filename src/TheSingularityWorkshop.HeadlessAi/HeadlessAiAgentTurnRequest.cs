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

    /// <summary>The exact serialized envelope shared with every provider.</summary>
    public string CanonicalPayload { get; }

    /// <summary>Lowercase SHA-256 fingerprint of <see cref="CanonicalPayload"/>.</summary>
    public string Fingerprint { get; }

    internal HeadlessAiAgentTurnEnvelope Envelope => _envelope;

    private HeadlessAiAgentTurnRequest(
        HeadlessAiAgentTurnEnvelope envelope,
        string canonicalPayload,
        string fingerprint)
    {
        _envelope = envelope;
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

        var canonicalPayload = JsonSerializer.Serialize(envelope, JsonOptions);
        if (canonicalPayload.Length > envelope.Limits.MaxSerializedEnvelopeCharacters)
            throw new ArgumentException("Serialized turn envelope exceeds the configured character limit.", nameof(envelope));

        var snapshot = JsonSerializer.Deserialize<HeadlessAiAgentTurnEnvelope>(canonicalPayload, JsonOptions)
            ?? throw new InvalidOperationException("The validated turn envelope could not be snapshotted.");
        snapshot.Validate(now);

        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayload)))
            .ToLowerInvariant();
        return new HeadlessAiAgentTurnRequest(snapshot, canonicalPayload, fingerprint);
    }
}
