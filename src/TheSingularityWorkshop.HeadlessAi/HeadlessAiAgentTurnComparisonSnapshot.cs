using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// An immutable canonical snapshot of a validated multi-provider comparison. The snapshot retains
/// the exact comparison payload and fingerprint without exposing its mutable nested record graph.
/// It does not infer consensus or promote provider assessments into verified facts.
/// </summary>
public sealed class HeadlessAiAgentTurnComparisonSnapshot
{
    private const int MaxSerializedCharacters = 40_000_000;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 32
    };

    private readonly HeadlessAiAgentTurnComparison _comparison;

    /// <summary>The exact serialized comparison shared with downstream consumers.</summary>
    public string CanonicalPayload { get; }

    /// <summary>Lowercase SHA-256 fingerprint of <see cref="CanonicalPayload"/>.</summary>
    public string Fingerprint { get; }

    internal HeadlessAiAgentTurnComparison Comparison => _comparison;

    private HeadlessAiAgentTurnComparisonSnapshot(
        HeadlessAiAgentTurnComparison comparison,
        string canonicalPayload,
        string fingerprint)
    {
        _comparison = comparison;
        CanonicalPayload = canonicalPayload;
        Fingerprint = fingerprint;
    }

    /// <summary>
    /// Validates and snapshots a comparison. Later mutations to caller-owned collections, provider
    /// results, proposals, metadata, evidence, findings, or relationship IDs cannot change this snapshot.
    /// </summary>
    public static HeadlessAiAgentTurnComparisonSnapshot Create(HeadlessAiAgentTurnComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        comparison.Validate();

        var boundedWriter = new BoundedBufferWriter(MaxSerializedCharacters);
        JsonSerializer.Serialize(boundedWriter, comparison, JsonOptions);
        var canonicalPayload = Encoding.UTF8.GetString(boundedWriter.WrittenSpan);
        if (canonicalPayload.Length > MaxSerializedCharacters)
            throw new ArgumentException("Serialized comparison exceeds the 40-million-character snapshot limit.", nameof(comparison));

        var snapshot = JsonSerializer.Deserialize<HeadlessAiAgentTurnComparison>(canonicalPayload, JsonOptions)
            ?? throw new InvalidOperationException("The validated comparison could not be snapshotted.");
        snapshot.Validate();

        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayload)))
            .ToLowerInvariant();
        return new HeadlessAiAgentTurnComparisonSnapshot(snapshot, canonicalPayload, fingerprint);
    }

    private sealed class BoundedBufferWriter : IBufferWriter<byte>
    {
        private readonly int _maximumBytes;
        private byte[] _buffer = new byte[16 * 1024];
        private int _written;

        public BoundedBufferWriter(int maximumBytes)
        {
            _maximumBytes = maximumBytes;
        }

        public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _written);

        public void Advance(int count)
        {
            if (count < 0 || count > _buffer.Length - _written)
                throw new ArgumentOutOfRangeException(nameof(count));
            _written += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return _buffer.AsMemory(_written);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return _buffer.AsSpan(_written);
        }

        private void EnsureCapacity(int sizeHint)
        {
            if (sizeHint < 0)
                throw new ArgumentOutOfRangeException(nameof(sizeHint));

            var required = checked(_written + Math.Max(sizeHint, 1));
            if (required > _maximumBytes)
                throw new ArgumentException(
                    "Serialized comparison exceeds the 40-million-character snapshot limit.");

            if (required <= _buffer.Length)
                return;

            var doubled = Math.Min(_maximumBytes, _buffer.Length * 2);
            Array.Resize(ref _buffer, Math.Max(required, doubled));
        }
    }
}
