using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Executes exactly one provider invocation for an explicit turn envelope. It owns no conversation
/// store, retry loop, task lifecycle, permission system, or action executor; the host owns those policies.
/// </summary>
public sealed class HeadlessAiAgentTurnExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 32
    };

    private readonly HeadlessAiAgentTemplate _template;
    private readonly HttpClient _httpClient;
    private readonly TimeProvider _timeProvider;

    /// <summary>Creates a stateless turn executor over a configured provider template and host-owned transport.</summary>
    /// <param name="template">Provider profile, protocol adapter, and optional host credential provider.</param>
    /// <param name="httpClient">Shared HTTP transport owned by the host.</param>
    /// <param name="timeProvider">Optional clock used for deterministic timestamps.</param>
    public HeadlessAiAgentTurnExecutor(HeadlessAiAgentTemplate template, HttpClient httpClient,
        TimeProvider? timeProvider = null)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Sends the complete, self-describing envelope as the canonical provider input and returns a
    /// provenance-bound result. A malformed model response is retained as raw data and produces
    /// an error result with no actionable proposal.
    /// </summary>
    /// <param name="envelope">Validated task turn, including bounded evidence references.</param>
    /// <param name="cancellationToken">Host cancellation token.</param>
    /// <returns>A normalized exchange result; provider and transport failures are represented as error results.</returns>
    public async Task<HeadlessAiAgentTurnResult> ExecuteAsync(
        HeadlessAiAgentTurnEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var startedAt = _timeProvider.GetUtcNow();
        envelope.Validate(startedAt);

        var canonicalRequest = JsonSerializer.Serialize(envelope, JsonOptions);
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest))).ToLowerInvariant();
        var providerId = _template.Adapter.Id;
        var modelId = _template.Profile.Settings.TryGetValue("model", out var configuredModel) &&
            !string.IsNullOrWhiteSpace(configuredModel)
                ? configuredModel
                : _template.Profile.Id;
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(envelope.Limits.Timeout);

        try
        {
            var agent = _template.CreateAgent(_httpClient);
            var output = await agent.SendAsync(new HeadlessAiInput(canonicalRequest), timeoutSource.Token)
                .ConfigureAwait(false);
            var completedAt = _timeProvider.GetUtcNow();
            return CreateSuccess(envelope, output, fingerprint, providerId, modelId, startedAt, completedAt);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt, "timeout",
                "The turn exceeded its configured timeout.", string.Empty);
        }
        catch (TimeoutException ex)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt, "timeout",
                ex.Message, string.Empty);
        }
        catch (HeadlessAiHttpException ex)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                ex.StatusCode is { } statusCode ? $"http_{(int)statusCode}" : "http_error",
                ex.Message, ex.ResponseExcerpt);
        }
        catch (HttpRequestException ex)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                "transport_error", ex.Message, string.Empty);
        }
        catch (InvalidDataException ex)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                "response_limit_exceeded", ex.Message, string.Empty);
        }
    }

    private HeadlessAiAgentTurnResult CreateSuccess(HeadlessAiAgentTurnEnvelope envelope, HeadlessAiOutput output,
        string fingerprint, string providerId, string modelId,
        DateTimeOffset startedAt, DateTimeOffset completedAt)
    {
        HeadlessAiAgentTurnProposal proposal;
        string? errorCode = null;
        string? diagnostic = null;
        try
        {
            proposal = JsonSerializer.Deserialize<HeadlessAiAgentTurnProposal>(output.Content, JsonOptions)
                ?? throw new JsonException("The provider returned an empty proposal.");
            proposal.Validate();
        }
        catch (JsonException ex)
        {
            proposal = new HeadlessAiAgentTurnProposal
            {
                Uncertainty = "The provider response could not be parsed as the requested proposal schema.",
                StopOrEscalateReason = "A malformed provider response must be reviewed; no action is proposed."
            };
            errorCode = "malformed_response";
            diagnostic = ex.Message;
        }
        catch (ArgumentException ex)
        {
            proposal = new HeadlessAiAgentTurnProposal
            {
                Uncertainty = "The provider response failed proposal validation.",
                StopOrEscalateReason = "An invalid provider proposal must be reviewed; no action is proposed."
            };
            errorCode = "invalid_proposal";
            diagnostic = ex.Message;
        }

        var metadata = output.Metadata?.ToDictionary(pair => pair.Key, pair => pair.Value,
            StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        return new HeadlessAiAgentTurnResult
        {
            TurnId = Guid.NewGuid().ToString("N"),
            TaskId = envelope.TaskId,
            CorrelationId = envelope.CorrelationId,
            ProviderId = providerId,
            ModelId = modelId,
            RequestFingerprint = fingerprint,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            RawResponse = output.Content,
            Proposal = proposal,
            ProviderMetadata = metadata,
            FinishReason = errorCode is null ? "completed" : "invalid_response",
            Diagnostic = diagnostic,
            ErrorCode = errorCode,
            InputTokens = ReadTokenCount(metadata, "input_tokens", "prompt_tokens"),
            OutputTokens = ReadTokenCount(metadata, "output_tokens", "completion_tokens")
        };
    }

    private HeadlessAiAgentTurnResult CreateFailure(HeadlessAiAgentTurnEnvelope envelope, string fingerprint,
        string providerId, string modelId, DateTimeOffset startedAt, string errorCode, string diagnostic, string rawResponse)
        => new()
        {
            TurnId = Guid.NewGuid().ToString("N"),
            TaskId = envelope.TaskId,
            CorrelationId = envelope.CorrelationId,
            ProviderId = providerId,
            ModelId = modelId,
            RequestFingerprint = fingerprint,
            StartedAt = startedAt,
            CompletedAt = _timeProvider.GetUtcNow(),
            RawResponse = rawResponse.Length <= 1_000_000 ? rawResponse : rawResponse[..1_000_000],
            Proposal = new HeadlessAiAgentTurnProposal
            {
                Uncertainty = "The provider invocation did not produce a usable proposal.",
                StopOrEscalateReason = "The host must decide whether to retry, stop, or escalate."
            },
            FinishReason = "error",
            Diagnostic = diagnostic,
            ErrorCode = errorCode
        };

    private static long? ReadTokenCount(IReadOnlyDictionary<string, string> metadata, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (metadata.TryGetValue(key, out var value) && long.TryParse(value, out var count) && count >= 0)
                return count;
        }
        return null;
    }
}
