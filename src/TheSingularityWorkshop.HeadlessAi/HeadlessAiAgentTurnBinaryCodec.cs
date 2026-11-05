using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.FSM_Serialization;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Reads and writes versioned binary frames for immutable agent-turn snapshots.
/// The frame is binary; its bounded body is the snapshot's existing canonical JSON.
/// Provider requests and their SHA-256 provenance fingerprints remain unchanged.
/// </summary>
public static class HeadlessAiAgentTurnBinaryCodec
{
    private const byte RequestFrameKind = 1;
    private const byte ComparisonFrameKind = 2;
    private const ushort FrameVersion = 1;
    private const int HeaderLength = 4 + 1 + sizeof(ushort) + sizeof(int) + 32;
    private const int MaxRequestPayloadBytes = 1_024_000;
    private const int MaxComparisonPayloadBytes = 40_000_000;
    private static ReadOnlySpan<byte> Magic => "HAIB"u8;

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 32
    };

    /// <summary>Writes a request snapshot as a versioned, integrity-checked binary frame.</summary>
    public static void PackRequest(HeadlessAiAgentTurnRequest request, IBinaryStream stream)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);
        WriteFrame(stream, RequestFrameKind, request.CanonicalPayload, MaxRequestPayloadBytes);
    }

    /// <summary>
    /// Reads and validates a request frame. The supplied time is used to reapply envelope expiry
    /// and lifetime rules; expired persisted requests cannot be resumed through this method.
    /// </summary>
    public static HeadlessAiAgentTurnRequest UnpackRequest(IBinaryStream stream, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var payload = ReadFrame(stream, RequestFrameKind, MaxRequestPayloadBytes);
        var envelope = JsonSerializer.Deserialize<HeadlessAiAgentTurnEnvelope>(payload, JsonOptions)
            ?? throw new InvalidDataException("The binary request frame contains an empty envelope.");
        var snapshot = HeadlessAiAgentTurnRequest.Create(envelope, now);
        EnsureCanonicalPayload(payload, snapshot.CanonicalPayload);
        return snapshot;
    }

    /// <summary>Writes a comparison snapshot as a versioned, integrity-checked binary frame.</summary>
    public static void PackComparison(HeadlessAiAgentTurnComparisonSnapshot comparison, IBinaryStream stream)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        ArgumentNullException.ThrowIfNull(stream);
        WriteFrame(stream, ComparisonFrameKind, comparison.CanonicalPayload, MaxComparisonPayloadBytes);
    }

    /// <summary>Reads and validates a comparison frame.</summary>
    public static HeadlessAiAgentTurnComparisonSnapshot UnpackComparison(IBinaryStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var payload = ReadFrame(stream, ComparisonFrameKind, MaxComparisonPayloadBytes);
        var comparison = JsonSerializer.Deserialize<HeadlessAiAgentTurnComparison>(payload, JsonOptions)
            ?? throw new InvalidDataException("The binary comparison frame contains an empty comparison.");
        var snapshot = HeadlessAiAgentTurnComparisonSnapshot.Create(comparison);
        EnsureCanonicalPayload(payload, snapshot.CanonicalPayload);
        return snapshot;
    }

    private static void WriteFrame(IBinaryStream stream, byte kind, string canonicalPayload, int maximumBytes)
    {
        if (!stream.CanWrite)
            throw new ArgumentException("The binary stream must support writing.", nameof(stream));

        var payload = StrictUtf8.GetBytes(canonicalPayload);
        if (payload.Length == 0 || payload.Length > maximumBytes)
            throw new ArgumentException("The canonical payload exceeds the binary frame limit.", nameof(canonicalPayload));

        Span<byte> header = stackalloc byte[HeaderLength];
        Magic.CopyTo(header);
        header[4] = kind;
        BinaryPrimitives.WriteUInt16LittleEndian(header[5..], FrameVersion);
        BinaryPrimitives.WriteInt32LittleEndian(header[7..], payload.Length);
        SHA256.HashData(payload, header[11..43]);
        stream.Write(header);
        stream.Write(payload);
    }

    private static byte[] ReadFrame(IBinaryStream stream, byte expectedKind, int maximumBytes)
    {
        if (!stream.CanRead)
            throw new ArgumentException("The binary stream must support reading.", nameof(stream));

        Span<byte> header = stackalloc byte[HeaderLength];
        ReadExactly(stream, header);
        if (!header[..4].SequenceEqual(Magic))
            throw new InvalidDataException("The binary frame magic is invalid.");
        if (header[4] != expectedKind)
            throw new InvalidDataException("The binary frame contains a different snapshot type.");
        if (BinaryPrimitives.ReadUInt16LittleEndian(header[5..]) != FrameVersion)
            throw new InvalidDataException("The binary frame version is not supported.");

        var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(header[7..]);
        if (payloadLength <= 0 || payloadLength > maximumBytes)
            throw new InvalidDataException("The binary frame payload length is outside the supported bounds.");

        var expectedDigest = header[11..43].ToArray();
        var payload = new byte[payloadLength];
        ReadExactly(stream, payload);
        var actualDigest = SHA256.HashData(payload);
        if (!CryptographicOperations.FixedTimeEquals(expectedDigest, actualDigest))
            throw new InvalidDataException("The binary frame payload integrity check failed.");

        try
        {
            _ = StrictUtf8.GetCharCount(payload);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException("The binary frame payload is not valid UTF-8.", exception);
        }

        return payload;
    }

    private static void ReadExactly(IBinaryStream stream, Span<byte> destination)
    {
        var offset = 0;
        while (offset < destination.Length)
        {
            var read = stream.Read(destination[offset..]);
            if (read <= 0)
                throw new EndOfStreamException("The binary frame ended before the complete field was read.");
            if (read > destination.Length - offset)
                throw new InvalidDataException("The binary stream reported more bytes than the supplied buffer can hold.");
            offset += read;
        }
    }

    private static void ReadExactly(IBinaryStream stream, byte[] destination)
        => ReadExactly(stream, destination.AsSpan());

    private static void EnsureCanonicalPayload(byte[] originalPayload, string reconstructedPayload)
    {
        var reconstructedBytes = StrictUtf8.GetBytes(reconstructedPayload);
        if (!originalPayload.AsSpan().SequenceEqual(reconstructedBytes))
            throw new InvalidDataException("The binary frame does not contain the canonical snapshot representation.");
    }
}
