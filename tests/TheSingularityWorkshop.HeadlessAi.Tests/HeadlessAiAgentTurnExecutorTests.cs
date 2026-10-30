using System.Net;
using System.Text;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiAgentTurnExecutorTests
{
    private static HeadlessAiAgentTurnEnvelope CreateEnvelope(TimeSpan? timeout = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new HeadlessAiAgentTurnEnvelope
        {
            TaskId = "task-100",
            CorrelationId = "correlation-100",
            Repository = "TrentBest/HeadlessAi.Consumer",
            AuthorizedBranch = "agent/repair-ci",
            BaseCommitSha = "0123456789abcdef0123456789abcdef01234567",
            Role = "repair-agent",
            Instructions = "Diagnose the supplied CI failure and propose a patch intent.",
            CanonicalPrompt = "A test failed. Return a structured proposal.",
            ExpiresAt = now.AddMinutes(10),
            Limits = new HeadlessAiAgentTurnLimits
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(5)
            }
        };
    }

    private static HeadlessAiAgentTurnExecutor CreateExecutor(
        HttpClient client, string adapterId = "fixture-provider")
    {
        var profile = new HeadlessAiProfile(
            "fixture-profile",
            new Uri("https://provider.example.test/invoke"),
            settings: new Dictionary<string, string> { ["model"] = "fixture-model" });
        var template = new HeadlessAiAgentTemplate(profile, new FixtureAdapter(adapterId));
        return new HeadlessAiAgentTurnExecutor(template, client);
    }

    [Fact]
    public async Task ExecuteAsync_SendsCanonicalEnvelopeAndPreservesRawAndNormalizedResponse()
    {
        const string raw = "{\"diagnosis\":\"assertion mismatch\",\"patchIntent\":\"correct expected value\",\"confidence\":0.9}";
        string? requestBody = null;
        var handler = new StubHandler(async (request, cancellationToken) =>
        {
            requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(raw, Encoding.UTF8, "application/json")
            };
        });
        using var client = new HttpClient(handler);
        var profile = new HeadlessAiProfile("fixture-profile", new Uri("https://provider.example.test/invoke"),
            settings: new Dictionary<string, string> { ["model"] = "fixture-model" });
        var executor = new HeadlessAiAgentTurnExecutor(
            new HeadlessAiAgentTemplate(profile, new FixtureAdapter("fixture-provider")), client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Contains("\"taskId\":\"task-100\"", requestBody);
        Assert.Contains("\"repository\":\"TrentBest/HeadlessAi.Consumer\"", requestBody);
        Assert.Equal(raw, result.RawResponse);
        Assert.Equal("assertion mismatch", result.Proposal.Diagnosis);
        Assert.Equal(0.9, result.Proposal.Confidence);
        Assert.Equal("fixture-provider", result.ProviderId);
        Assert.Equal("fixture-model", result.ModelId);
        Assert.Equal("v1", result.ProviderVersion);
        Assert.Equal("stop", result.FinishReason);
        Assert.Equal(12, result.InputTokens);
        Assert.Equal(8, result.OutputTokens);
        Assert.Equal(64, result.RequestFingerprint.Length);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_HttpProviderFailureIsNormalizedWithoutAutomaticRetry()
    {
        var attempts = 0;
        using var client = new HttpClient(new StubHandler((_, _) =>
        {
            attempts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("rate limited")
            });
        }));
        var executor = CreateExecutor(client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Equal(1, attempts);
        Assert.Equal("http_429", result.ErrorCode);
        Assert.Equal("rate limited", result.RawResponse);
        Assert.Null(result.Proposal.PatchIntent);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_ResumesFromExplicitPriorTurnAndCandidateEvidence()
    {
        var requests = new List<string>();
        const string raw = "{\"diagnosis\":\"continued from prior CI attempt\"}";
        using var client = new HttpClient(new StubHandler(async (request, cancellationToken) =>
        {
            requests.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(raw)
            };
        }));
        var executor = CreateExecutor(client);
        var firstEnvelope = CreateEnvelope() with { IdempotencyKey = "task-100-repair-1" };
        var firstResult = await executor.ExecuteAsync(firstEnvelope);
        const string priorSha = "0123456789abcdef0123456789abcdef01234567";
        const string nextSha = "abcdefabcdefabcdefabcdefabcdefabcdefabcd";
        var resumedEnvelope = CreateEnvelope() with
        {
            CandidateCommitSha = nextSha,
            PriorTurnId = firstResult.TurnId,
            PriorCandidateCommitSha = priorSha,
            IdempotencyKey = "task-100-repair-2",
            Evidence =
            [
                new HeadlessAiEvidenceReference
                {
                    Repository = "TrentBest/HeadlessAi.Consumer",
                    CommitSha = priorSha,
                    WorkflowRunId = "run-200",
                    Attempt = 2,
                    WorkflowName = "Build and Test",
                    JobName = "test",
                    Conclusion = "failure",
                    SourceUrl = "https://github.com/TrentBest/HeadlessAi.Consumer/actions/runs/200",
                    ObservedAt = DateTimeOffset.UtcNow,
                    BoundedSummary = "The previous candidate failed one test."
                }
            ]
        };

        var resumedResult = await executor.ExecuteAsync(resumedEnvelope);

        Assert.Equal(firstResult.TaskId, resumedResult.TaskId);
        Assert.Equal(firstResult.CorrelationId, resumedResult.CorrelationId);
        Assert.Contains(firstResult.TurnId, requests[1]);
        Assert.Contains(priorSha, requests[1]);
        Assert.Contains("run-200", requests[1]);
        Assert.Equal(nextSha, resumedResult.CandidateCommitSha);
        Assert.Equal(firstResult.TurnId, resumedResult.PriorTurnId);
        Assert.Single(resumedResult.Evidence);
        Assert.Equal(priorSha, resumedResult.Evidence[0].CommitSha);
        Assert.NotEqual(firstResult.RequestFingerprint, resumedResult.RequestFingerprint);
    }

    [Fact]
    public async Task ExecuteAsync_MalformedProviderJsonIsPreservedButProducesNoActionableProposal()
    {
        const string raw = "not-json";
        using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(raw) })));
        var executor = CreateExecutor(client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Equal(raw, result.RawResponse);
        Assert.Equal("malformed_response", result.ErrorCode);
        Assert.Null(result.Proposal.PatchIntent);
        Assert.NotNull(result.Proposal.StopOrEscalateReason);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutReturnsBoundedFailureResultWithoutRetrying()
    {
        var attempts = 0;
        using var client = new HttpClient(new StubHandler(async (_, cancellationToken) =>
        {
            attempts++;
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }));
        var executor = CreateExecutor(client);

        var result = await executor.ExecuteAsync(CreateEnvelope(TimeSpan.FromMilliseconds(20)));

        Assert.Equal(1, attempts);
        Assert.Equal("timeout", result.ErrorCode);
        Assert.Equal("error", result.FinishReason);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_RejectsEnvelopeAboveSerializedCharacterLimitBeforeSending()
    {
        var attempts = 0;
        using var client = new HttpClient(new StubHandler((_, _) =>
        {
            attempts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"diagnosis":"unexpected"}""")
            });
        }));
        var executor = CreateExecutor(client);
        var envelope = CreateEnvelope() with
        {
            Limits = new HeadlessAiAgentTurnLimits { MaxSerializedEnvelopeCharacters = 10 }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => executor.ExecuteAsync(envelope));

        Assert.Equal(0, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCallerCancellation()
    {
        using var client = new HttpClient(new StubHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }));
        var executor = CreateExecutor(client);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(CreateEnvelope(), cancellation.Token));
    }

    [Fact]
    public async Task ExecuteAsync_SharedImmutableRequestKeepsIdenticalPayloadAcrossProviders()
    {
        const string raw = "{\"diagnosis\":\"same input\"}";
        string? firstPayload = null;
        string? secondPayload = null;
        using var firstClient = new HttpClient(new StubHandler(async (request, cancellationToken) =>
        {
            firstPayload = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(raw) };
        }));
        using var secondClient = new HttpClient(new StubHandler(async (request, cancellationToken) =>
        {
            secondPayload = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(raw) };
        }));
        var first = CreateExecutor(firstClient, "provider-a");
        var second = CreateExecutor(secondClient, "provider-b");
        var contextReferences = new List<string> { "context://original" };
        var candidateSha = "abcdefabcdefabcdefabcdefabcdefabcdefabcd";
        var evidence = new List<HeadlessAiEvidenceReference>
        {
            new()
            {
                Repository = "TrentBest/HeadlessAi.Consumer",
                CommitSha = candidateSha,
                WorkflowRunId = "run-snapshot",
                Attempt = 1,
                WorkflowName = "Build and Test",
                Conclusion = "failure",
                SourceUrl = "https://github.com/TrentBest/HeadlessAi.Consumer/actions/runs/100",
                ObservedAt = DateTimeOffset.UtcNow,
                BoundedSummary = "original evidence"
            }
        };
        var envelope = CreateEnvelope() with
        {
            CandidateCommitSha = candidateSha,
            ContextReferences = contextReferences,
            Evidence = evidence
        };
        var request = HeadlessAiAgentTurnRequest.Create(envelope, DateTimeOffset.UtcNow);

        contextReferences[0] = "context://mutated";
        contextReferences.Add("context://added-after-snapshot");
        evidence[0] = evidence[0] with { BoundedSummary = "mutated evidence" };
        evidence.Add(evidence[0]);

        var firstResult = await first.ExecuteAsync(request);
        var secondResult = await second.ExecuteAsync(request);

        Assert.Equal(request.CanonicalPayload, firstPayload);
        Assert.Equal(firstPayload, secondPayload);
        Assert.Contains("context://original", request.CanonicalPayload);
        Assert.DoesNotContain("context://mutated", request.CanonicalPayload);
        Assert.DoesNotContain("context://added-after-snapshot", request.CanonicalPayload);
        Assert.Contains("original evidence", request.CanonicalPayload);
        Assert.DoesNotContain("mutated evidence", request.CanonicalPayload);
        Assert.Single(firstResult.Evidence);
        Assert.Equal("original evidence", firstResult.Evidence[0].BoundedSummary);
        Assert.Equal(request.Fingerprint, firstResult.RequestFingerprint);
        Assert.Equal(firstResult.RequestFingerprint, secondResult.RequestFingerprint);
        Assert.NotEqual(firstResult.ProviderId, secondResult.ProviderId);
    }

    [Fact]
    public async Task ExecuteAsync_PreservesCompleteCodeRedOperationalPayload()
    {
        const string raw = """{"operationalStatus":"blocked","currentAction":"Waiting for CI credentials","blocker":"The host secret is unavailable","nextExpectedUpdate":"After the host confirms secret restoration","stopOrEscalateReason":"The provider cannot continue without host authorization","humanDecisionRequired":"Restore the CI secret through approved settings"}""";
        using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(raw) })));
        var executor = CreateExecutor(client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Null(result.ErrorCode);
        Assert.Equal("blocked", result.Proposal.OperationalStatus);
        Assert.Equal("Waiting for CI credentials", result.Proposal.CurrentAction);
        Assert.Equal("The host secret is unavailable", result.Proposal.Blocker);
        Assert.Equal("After the host confirms secret restoration", result.Proposal.NextExpectedUpdate);
        Assert.Equal("The provider cannot continue without host authorization", result.Proposal.StopOrEscalateReason);
        Assert.Equal("Restore the CI secret through approved settings", result.Proposal.HumanDecisionRequired);
        Assert.Equal(raw, result.RawResponse);
        result.Validate();
    }

    private sealed class FixtureAdapter(
        string id,
        IReadOnlyDictionary<string, string>? metadata = null) : IHeadlessAiAdapter
    {
        public string Id { get; } = id;
        private readonly IReadOnlyDictionary<string, string> _metadata = metadata ??
            new Dictionary<string, string>
            {
                ["response_id"] = "fixture-response",
                ["input_tokens"] = "12",
                ["output_tokens"] = "8",
                ["provider_version"] = "v1",
                ["finish_reason"] = "stop"
            };

        public HttpRequestMessage CreateRequest(HeadlessAiProfile profile, HeadlessAiInput input)
            => new(HttpMethod.Post, profile.Endpoint)
            {
                Content = new StringContent(input.Content, Encoding.UTF8, "application/json")
            };

        public async ValueTask<HeadlessAiOutput> ReadResponseAsync(
            HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return new HeadlessAiOutput(content, response.StatusCode, response.Content.Headers.ContentType?.MediaType,
                _metadata);
        }
    }

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_RepeatedDeliveryPreservesIdempotencyProvenanceWithoutClaimingDeduplication()
    {
        var invocationCount = 0;
        using var client = new HttpClient(new StubHandler((_, _) =>
        {
            invocationCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"diagnosis":"same canonical request"}""")
            });
        }));
        var executor = CreateExecutor(client);
        var envelope = CreateEnvelope() with { IdempotencyKey = "task-100-delivery-1" };

        var first = await executor.ExecuteAsync(envelope);
        var redelivery = await executor.ExecuteAsync(envelope);

        Assert.Equal(2, invocationCount);
        Assert.Equal(envelope.IdempotencyKey, first.IdempotencyKey);
        Assert.Equal(first.IdempotencyKey, redelivery.IdempotencyKey);
        Assert.Equal(first.RequestFingerprint, redelivery.RequestFingerprint);
        Assert.NotEqual(first.TurnId, redelivery.TurnId);
        Assert.Equal(first.TaskId, redelivery.TaskId);
        Assert.Equal(first.CorrelationId, redelivery.CorrelationId);
        first.Validate();
        redelivery.Validate();
    }


    [Fact]
    public async Task ExecuteAsync_CaseCollidingProviderMetadataIsNormalizedWithoutThrowing()
    {
        using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"diagnosis":"metadata is supplemental"}""")
            })));
        var metadata = new Dictionary<string, string>
        {
            ["finish_reason"] = "first-value",
            ["FINISH_REASON"] = "second-value",
            ["provider_version"] = "fixture-v2"
        };
        var template = new HeadlessAiAgentTemplate(
            new HeadlessAiProfile("fixture-profile", new Uri("https://provider.example.test/invoke")),
            new FixtureAdapter("fixture-provider", metadata));
        var executor = new HeadlessAiAgentTurnExecutor(template, client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Null(result.ErrorCode);
        Assert.Equal("first-value", result.FinishReason);
        Assert.Equal("fixture-v2", result.ProviderVersion);
        Assert.Equal("first-value", result.ProviderMetadata["finish_reason"]);
        Assert.Single(result.ProviderMetadata.Keys.Where(key =>
            string.Equals(key, "finish_reason", StringComparison.OrdinalIgnoreCase)));
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_TransportFailureIsNormalizedWithoutRawExceptionPayload()
    {
        using var client = new HttpClient(new StubHandler((_, _) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("connection unavailable"))));
        var executor = CreateExecutor(client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Equal("transport_error", result.ErrorCode);
        Assert.Equal("error", result.FinishReason);
        Assert.Equal(string.Empty, result.RawResponse);
        Assert.Contains("connection unavailable", result.Diagnostic);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutExceptionIsNormalized()
    {
        using var client = new HttpClient(new StubHandler((_, _) =>
            Task.FromException<HttpResponseMessage>(new TimeoutException("provider deadline elapsed"))));
        var executor = CreateExecutor(client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Equal("timeout", result.ErrorCode);
        Assert.Contains("provider deadline elapsed", result.Diagnostic);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_ResponseBodyAboveProfileLimitIsNormalized()
    {
        using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("response exceeds eight bytes")
            })));
        var profile = new HeadlessAiProfile("fixture-profile", new Uri("https://provider.example.test/invoke"),
            settings: new Dictionary<string, string> { ["model"] = "fixture-model" }, maxResponseBytes: 8);
        var executor = new HeadlessAiAgentTurnExecutor(
            new HeadlessAiAgentTemplate(profile, new FixtureAdapter("fixture-provider")), client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Equal("response_limit_exceeded", result.ErrorCode);
        Assert.Equal(string.Empty, result.RawResponse);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_InvalidNormalizedProposalIsRetainedAsNonActionableFailure()
    {
        const string raw = """{"confidence":1.5,"patchIntent":"unsafe proposal"}""";
        using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(raw) })));
        var executor = CreateExecutor(client);

        var result = await executor.ExecuteAsync(CreateEnvelope());

        Assert.Equal(raw, result.RawResponse);
        Assert.Equal("invalid_proposal", result.ErrorCode);
        Assert.Null(result.Proposal.PatchIntent);
        Assert.NotNull(result.Proposal.StopOrEscalateReason);
        result.Validate();
    }

}
