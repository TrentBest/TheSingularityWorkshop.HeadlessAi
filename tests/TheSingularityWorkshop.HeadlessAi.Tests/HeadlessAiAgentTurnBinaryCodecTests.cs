using System.Buffers.Binary;
using TheSingularityWorkshop.FSM_Serialization;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiAgentTurnBinaryCodecTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
    private const string Sha = "0123456789abcdef0123456789abcdef01234567";
    private const string Fingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    private static HeadlessAiAgentTurnEnvelope Envelope() => new()
    {
        TaskId = "task-1",
        ConversationId = "conversation-1",
        TaskVersion = "task-v1",
        CorrelationId = "correlation-1",
        Repository = "TrentBest/Example",
        AuthorizedBranch = "agent/fix",
        BaseCommitSha = Sha,
        Role = "implementation",
        Instructions = "Inspect and report.",
        CanonicalPrompt = "Review the candidate.",
        ExpiresAt = Now.AddMinutes(30)
    };

    private static HeadlessAiAgentTurnResult Result(string provider, string turnId) => new()
    {
        TurnId = turnId,
        TaskId = "task-1",
        ConversationId = "conversation-1",
        TaskVersion = "task-v1",
        CorrelationId = "correlation-1",
        Repository = "TrentBest/Example",
        AuthorizedBranch = "agent/fix",
        BaseCommitSha = Sha,
        ProviderId = provider,
        ModelId = provider + "-model",
        RequestFingerprint = Fingerprint,
        StartedAt = Now,
        CompletedAt = Now.AddSeconds(1),
        RawResponse = "{}",
        Proposal = new HeadlessAiAgentTurnProposal { Diagnosis = "A provider-specific diagnosis." },
        FinishReason = "completed"
    };

    private static HeadlessAiAgentTurnComparison Comparison() => new()
    {
        TaskId = "task-1",
        ConversationId = "conversation-1",
        TaskVersion = "task-v1",
        CorrelationId = "correlation-1",
        Repository = "TrentBest/Example",
        RequestFingerprint = Fingerprint,
        ProviderResults = [Result("provider-a", "turn-a"), Result("provider-b", "turn-b")]
    };

    [Fact]
    public void RequestFrame_RoundTripsWithoutChangingCanonicalPayloadOrFingerprint()
    {
        var original = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();

        HeadlessAiAgentTurnBinaryCodec.PackRequest(original, stream);
        stream.Position = 0;
        var restored = HeadlessAiAgentTurnBinaryCodec.UnpackRequest(stream, Now);

        Assert.Equal(original.CanonicalPayload, restored.CanonicalPayload);
        Assert.Equal(original.Fingerprint, restored.Fingerprint);
    }

    [Fact]
    public void RequestFrame_IsDeterministicForTheSameSnapshot()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var first = new MemoryBinaryStream();
        using var second = new MemoryBinaryStream();

        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, first);
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, second);

        Assert.Equal(first.ToArray(), second.ToArray());
    }

    [Fact]
    public void ComparisonFrame_RoundTripsWithoutChangingCanonicalPayloadOrFingerprint()
    {
        var original = HeadlessAiAgentTurnComparisonSnapshot.Create(Comparison());
        using var stream = new MemoryBinaryStream();

        HeadlessAiAgentTurnBinaryCodec.PackComparison(original, stream);
        stream.Position = 0;
        var restored = HeadlessAiAgentTurnBinaryCodec.UnpackComparison(stream);

        Assert.Equal(original.CanonicalPayload, restored.CanonicalPayload);
        Assert.Equal(original.Fingerprint, restored.Fingerprint);
    }

    [Fact]
    public void Frame_RejectsUnsupportedVersion()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, stream);
        var bytes = stream.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(5, 2), 99);
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(corrupted, Now));
    }

    [Fact]
    public void Frame_RejectsWrongSnapshotKind()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, stream);
        stream.Position = 0;

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(stream));
    }

    [Fact]
    public void Frame_RejectsTruncatedPayload()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, stream);
        var bytes = stream.ToArray();
        using var truncated = new MemoryBinaryStream(bytes[..^1]);

        Assert.Throws<EndOfStreamException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(truncated, Now));
    }

    [Fact]
    public void Frame_RejectsPayloadTampering()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, stream);
        var bytes = stream.ToArray();
        bytes[^1] ^= 0x01;
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(corrupted, Now));
    }

    [Fact]
    public void RequestFrame_RejectsExpiredEnvelope()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, stream);
        stream.Position = 0;

        Assert.Throws<ArgumentException>(() =>
            HeadlessAiAgentTurnBinaryCodec.UnpackRequest(stream, Now.AddHours(1)));
    }
}
