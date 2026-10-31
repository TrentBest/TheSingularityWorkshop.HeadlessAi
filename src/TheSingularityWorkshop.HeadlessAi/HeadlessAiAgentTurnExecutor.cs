using System.Net;
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
    public Task<HeadlessAiAgentTurnResult> ExecuteAsync(
        HeadlessAiAgentTurnEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var request = HeadlessAiAgentTurnRequest.Create(envelope, _timeProvider.GetUtcNow());
        return ExecuteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Executes a previously created immutable request snapshot. Share the same request instance
    /// across provider executors to guarantee identical canonical input and fingerprint.
    /// </summary>
    /// <param name="request">Validated immutable canonical request snapshot.</param>
    /// <param name="cancellationToken">Host cancellation token.</param>
    /// <returns>A normalized exchange result bound to the shared request fingerprint.</returns>
    public async Task<HeadlessAiAgentTurnResult> ExecuteAsync(
        HeadlessAiAgentTurnRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var startedAt = _timeProvider.GetUtcNow();
        var envelope = request.Envelope;
        envelope.Validate(startedAt);

        var canonicalRequest = request.CanonicalPayload;
        var fingerprint = request.Fingerprint;
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
            if (output is null || output.Content is null)
                return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                    "invalid_adapter_response",
                    "The provider adapter returned no response content.", string.Empty);
            var completedAt = _timeProvider.GetUtcNow();
            if (IsCredentialMetadata("provider_response", output.Content))
                return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                    "sensitive_response",
                    "The provider response contained credential-shaped material and was discarded.", string.Empty);
            if (output.Content.Length > 1_000_000)
                return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                    "response_limit_exceeded",
                    "The provider response exceeds the one-million-character result limit.", string.Empty);
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
        catch (TimeoutException)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt, "timeout",
                "The provider request timed out.", string.Empty);
        }
        catch (HeadlessAiHttpException ex)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                ex.StatusCode is { } statusCode ? $"http_{(int)statusCode}" : "http_error",
                "The provider returned a non-success HTTP response.", SanitizeErrorResponseExcerpt(ex.ResponseExcerpt));
        }
        catch (HttpRequestException)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                "transport_error", "The provider transport request failed.", string.Empty);
        }
        catch (InvalidDataException)
        {
            return CreateFailure(envelope, fingerprint, providerId, modelId, startedAt,
                "response_limit_exceeded", "The provider response exceeded a configured size limit.", string.Empty);
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
        catch (JsonException)
        {
            proposal = new HeadlessAiAgentTurnProposal
            {
                Uncertainty = "The provider response could not be parsed as the requested proposal schema.",
                StopOrEscalateReason = "A malformed provider response must be reviewed; no action is proposed."
            };
            errorCode = "malformed_response";
            diagnostic = "The provider response was not valid JSON for the requested proposal schema.";
        }
        catch (ArgumentException)
        {
            proposal = new HeadlessAiAgentTurnProposal
            {
                Uncertainty = "The provider response failed proposal validation.",
                StopOrEscalateReason = "An invalid provider proposal must be reviewed; no action is proposed."
            };
            errorCode = "invalid_proposal";
            diagnostic = "The provider proposal failed contract validation.";
        }

        // Provider metadata is untrusted. Keep the first spelling when a provider supplies
        // keys that differ only by casing; a case-insensitive dictionary must not make an
        // otherwise successful invocation throw during result normalization.
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (output.Metadata is not null)
        {
            foreach (var pair in output.Metadata)
            {
                // Provider metadata is supplemental. Drop malformed or oversized entries
                // rather than allowing untrusted adapter data to invalidate the result.
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > 128 ||
                    pair.Value is null || pair.Value.Length > 4_096 ||
                    IsCredentialMetadata(pair.Key, pair.Value))
                    continue;
                if (metadata.ContainsKey(pair.Key))
                    continue;
                if (metadata.Count >= 128)
                    break;
                metadata.Add(pair.Key, pair.Value);
            }
        }
        return new HeadlessAiAgentTurnResult
        {
            TurnId = Guid.NewGuid().ToString("N"),
            TaskId = envelope.TaskId,
            ConversationId = envelope.ConversationId,
            TaskVersion = envelope.TaskVersion,
            CorrelationId = envelope.CorrelationId,
            EnvelopeVersion = envelope.Version,
            Repository = envelope.Repository,
            AuthorizedBranch = envelope.AuthorizedBranch,
            BaseCommitSha = envelope.BaseCommitSha,
            CandidateCommitSha = envelope.CandidateCommitSha,
            PriorTurnId = envelope.PriorTurnId,
            PriorCandidateCommitSha = envelope.PriorCandidateCommitSha,
            IdempotencyKey = envelope.IdempotencyKey,
            IssueUrl = envelope.IssueUrl,
            PullRequestUrl = envelope.PullRequestUrl,
            Evidence = envelope.Evidence.ToArray(),
            ProviderId = providerId,
            ModelId = modelId,
            ConfigurationId = _template.Profile.Id,
            ProviderVersion = metadata.TryGetValue("provider_version", out var providerVersion) ? providerVersion :
                metadata.TryGetValue("api_version", out var apiVersion) ? apiVersion : null,
            RequestFingerprint = fingerprint,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            RawResponse = output.Content,
            Proposal = proposal,
            ProviderMetadata = metadata,
            FinishReason = metadata.TryGetValue("finish_reason", out var finishReason) ? finishReason :
                metadata.TryGetValue("stop_reason", out var stopReason) ? stopReason :
                errorCode is null ? "completed" : "invalid_response",
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
            ConversationId = envelope.ConversationId,
            TaskVersion = envelope.TaskVersion,
            CorrelationId = envelope.CorrelationId,
            EnvelopeVersion = envelope.Version,
            Repository = envelope.Repository,
            AuthorizedBranch = envelope.AuthorizedBranch,
            BaseCommitSha = envelope.BaseCommitSha,
            CandidateCommitSha = envelope.CandidateCommitSha,
            PriorTurnId = envelope.PriorTurnId,
            PriorCandidateCommitSha = envelope.PriorCandidateCommitSha,
            IdempotencyKey = envelope.IdempotencyKey,
            IssueUrl = envelope.IssueUrl,
            PullRequestUrl = envelope.PullRequestUrl,
            Evidence = envelope.Evidence.ToArray(),
            ProviderId = providerId,
            ModelId = modelId,
            ConfigurationId = _template.Profile.Id,
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

    private static string SanitizeErrorResponseExcerpt(string? excerpt)
    {
        if (string.IsNullOrEmpty(excerpt) || IsCredentialMetadata("response_excerpt", excerpt))
            return string.Empty;

        return excerpt;
    }

    private static bool IsCredentialMetadata(string key, string value)
    {
        var normalizedKey = key.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal);

        if (normalizedKey.Contains("authorization", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("apikey", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("accesstoken", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("refreshtoken", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("clientsecret", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("credential", StringComparison.OrdinalIgnoreCase) ||
            normalizedKey.Contains("secret", StringComparison.OrdinalIgnoreCase))
            return true;

        return System.Text.RegularExpressions.Regex.IsMatch(value,
            @"(?i)\bBearer\s+\S{12,}|[?&](?:token|key|signature|sig|api[_-]?key|access[_-]?token|refresh[_-]?token)=([^&\s]{8,})|\b(?:api[_-]?key|access[_-]?token|client[_-]?secret|password)\s*[:=]\s*['"" ]?[^\s,'"" ]{8,}|['""](?:authorization|api[_-]?key|access[_-]?token|refresh[_-]?token|client[_-]?secret|password|credential|secret)['""]\s*:",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    }

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
