using System.Buffers.Binary;
using System.Security.Cryptography;
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
    [Fact]
    public void Frame_ReadsCorrectlyWhenTheStreamReturnsPartialChunks()
    {
        var original = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var packed = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(original, packed);
        using var partial = new PartialReadBinaryStream(packed.ToArray());

        var restored = HeadlessAiAgentTurnBinaryCodec.UnpackRequest(partial, Now);

        Assert.Equal(original.CanonicalPayload, restored.CanonicalPayload);
        Assert.Equal(original.Fingerprint, restored.Fingerprint);
    }

    [Fact]
    public void Frame_RejectsPayloadLengthAboveTheRequestLimitBeforeAllocating()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, stream);
        var bytes = stream.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(7, 4), 1_024_001);
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(corrupted, Now));
    }

    [Fact]
    public void Frame_RejectsInvalidUtf8EvenWhenTheDigestMatches()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, stream);
        var bytes = stream.ToArray();
        bytes[43] = 0xff;
        System.Security.Cryptography.SHA256.HashData(bytes.AsSpan(43), bytes.AsSpan(11, 32));
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(corrupted, Now));
    }

    [Fact]
    public void Frame_RejectsStreamThatReportsMoreBytesThanTheProvidedBuffer()
    {
        using var invalid = new OverReportingBinaryStream();

        Assert.Throws<InvalidDataException>(() =>
            HeadlessAiAgentTurnBinaryCodec.UnpackRequest(invalid, Now));
    }

    private sealed class OverReportingBinaryStream : IBinaryStream
    {
        public bool CanRead => true;
        public bool CanWrite => false;
        public long Position { get; set; }
        public long Length => 0;

        public int Read(Span<byte> buffer) => checked(buffer.Length + 1);

        public void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();
        public void Flush() { }
        public void Dispose() { }
    }

    private sealed class PartialReadBinaryStream : IBinaryStream
    {
        private readonly byte[] _bytes;
        private int _position;

        public PartialReadBinaryStream(byte[] bytes) => _bytes = bytes;

        public bool CanRead => true;
        public bool CanWrite => false;
        public long Position
        {
            get => _position;
            set => _position = checked((int)value);
        }
        public long Length => _bytes.Length;

        public int Read(Span<byte> buffer)
        {
            var count = Math.Min(2, Math.Min(buffer.Length, _bytes.Length - _position));
            if (count <= 0)
                return 0;
            _bytes.AsSpan(_position, count).CopyTo(buffer);
            _position += count;
            return count;
        }

        public void Write(ReadOnlySpan<byte> buffer)
            => throw new NotSupportedException();

        public void Flush()
        {
        }

        public void Dispose()
        {
        }
    }


    [Fact]
    public void Frame_RejectsInvalidMagic()
    {
        var bytes = PackRequestFrame();
        bytes[0] ^= 0x01;
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(corrupted, Now));
    }

    [Fact]
    public void Frame_RejectsUnknownFrameKind()
    {
        var bytes = PackRequestFrame();
        bytes[4] = 99;
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(corrupted, Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Frame_RejectsNonPositivePayloadLength(int length)
    {
        var bytes = PackRequestFrame();
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(7, 4), length);
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(corrupted, Now));
    }

    [Fact]
    public void Frame_RejectsNonCanonicalJsonEvenWhenDigestMatches()
    {
        var original = PackRequestFrame();
        var payload = original[43..].Concat(new byte[] { (byte)' ' }).ToArray();
        var framed = Reframe(original, payload);
        using var corrupted = new MemoryBinaryStream(framed);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(corrupted, Now));
    }

    [Fact]
    public void Frame_RejectsTruncatedHeader()
    {
        var bytes = PackRequestFrame();
        using var truncated = new MemoryBinaryStream(bytes[..20]);

        Assert.Throws<EndOfStreamException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(truncated, Now));
    }

    [Fact]
    public void RequestFrame_RejectsStreamThatDoesNotSupportWriting()
    {
        using var stream = new PartialReadBinaryStream(Array.Empty<byte>());

        Assert.Throws<ArgumentException>(() =>
            HeadlessAiAgentTurnBinaryCodec.PackRequest(HeadlessAiAgentTurnRequest.Create(Envelope(), Now), stream));
    }

    [Fact]
    public void RequestFrame_RejectsStreamThatDoesNotSupportReading()
    {
        using var stream = new WriteOnlyBinaryStream();

        Assert.Throws<ArgumentException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackRequest(stream, Now));
    }

    private static byte[] PackRequestFrame()
    {
        var snapshot = HeadlessAiAgentTurnRequest.Create(Envelope(), Now);
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackRequest(snapshot, stream);
        return stream.ToArray();
    }

    private static byte[] Reframe(byte[] originalFrame, byte[] payload)
    {
        var frame = new byte[43 + payload.Length];
        originalFrame.AsSpan(0, 43).CopyTo(frame);
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(7, 4), payload.Length);
        payload.CopyTo(frame, 43);
        SHA256.HashData(payload, frame.AsSpan(11, 32));
        return frame;
    }

    private sealed class WriteOnlyBinaryStream : IBinaryStream
    {
        public bool CanRead => false;
        public bool CanWrite => true;
        public long Position { get; set; }
        public long Length => 0;
        public int Read(Span<byte> buffer) => throw new NotSupportedException();
        public void Write(ReadOnlySpan<byte> buffer) { }
        public void Flush() { }
        public void Dispose() { }
    }


    [Fact]
    public void ComparisonFrame_RejectsUnsupportedVersion()
    {
        var bytes = PackComparisonFrame();
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(5, 2), 99);
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(corrupted));
    }

    [Fact]
    public void ComparisonFrame_RejectsPayloadTampering()
    {
        var bytes = PackComparisonFrame();
        bytes[^1] ^= 0x01;
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(corrupted));
    }

    [Fact]
    public void ComparisonFrame_RejectsRequestFrameKind()
    {
        var bytes = PackRequestFrame();
        using var wrongKind = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(wrongKind));
    }

    [Fact]
    public void ComparisonFrame_RejectsTruncatedHeaderAndPayload()
    {
        var bytes = PackComparisonFrame();
        using var truncatedHeader = new MemoryBinaryStream(bytes[..20]);
        using var truncatedPayload = new MemoryBinaryStream(bytes[..^1]);

        Assert.Throws<EndOfStreamException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(truncatedHeader));
        Assert.Throws<EndOfStreamException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(truncatedPayload));
    }

    [Fact]
    public void ComparisonFrame_RejectsInvalidUtf8EvenWhenDigestMatches()
    {
        var bytes = PackComparisonFrame();
        bytes[43] = 0xff;
        SHA256.HashData(bytes.AsSpan(43), bytes.AsSpan(11, 32));
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(corrupted));
    }

    [Fact]
    public void ComparisonFrame_RejectsPayloadLengthAboveTheComparisonLimitBeforeAllocating()
    {
        var bytes = PackComparisonFrame();
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(7, 4), 40_000_001);
        using var corrupted = new MemoryBinaryStream(bytes);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(corrupted));
    }

    [Fact]
    public void ComparisonFrame_RejectsNonCanonicalJsonEvenWhenDigestMatches()
    {
        var original = PackComparisonFrame();
        var payload = original[43..].Concat(new byte[] { (byte)' ' }).ToArray();
        var framed = Reframe(original, payload);
        using var corrupted = new MemoryBinaryStream(framed);

        Assert.Throws<InvalidDataException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(corrupted));
    }

    [Fact]
    public void ComparisonFrame_RejectsInvalidComparisonEvenWhenFrameDigestMatches()
    {
        var original = PackComparisonFrame();
        var json = System.Text.Encoding.UTF8.GetString(original, 43, original.Length - 43);
        const string originalCorrelation = "\"correlationId\":\"correlation-1\"";
        const string changedCorrelation = "\"correlationId\":\"different-correlation\"";
        var firstOccurrence = json.IndexOf(originalCorrelation, StringComparison.Ordinal);
        var secondOccurrence = json.IndexOf(originalCorrelation, firstOccurrence + originalCorrelation.Length, StringComparison.Ordinal);
        Assert.True(secondOccurrence >= 0);
        var alteredJson = json[..secondOccurrence] + changedCorrelation +
            json[(secondOccurrence + originalCorrelation.Length)..];
        var payload = System.Text.Encoding.UTF8.GetBytes(alteredJson);
        var framed = Reframe(original, payload);
        using var corrupted = new MemoryBinaryStream(framed);

        Assert.Throws<ArgumentException>(() => HeadlessAiAgentTurnBinaryCodec.UnpackComparison(corrupted));
    }

    private static byte[] PackComparisonFrame()
    {
        var snapshot = HeadlessAiAgentTurnComparisonSnapshot.Create(Comparison());
        using var stream = new MemoryBinaryStream();
        HeadlessAiAgentTurnBinaryCodec.PackComparison(snapshot, stream);
        return stream.ToArray();
    }

}
