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
        Assert.Equal(64, result.RequestFingerprint.Length);
        result.Validate();
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
    public async Task ExecuteAsync_UsesSameFingerprintForSameEnvelopeAcrossProviders()
    {
        const string raw = "{\"diagnosis\":\"same input\"}";
        using var firstClient = new HttpClient(new StubHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(raw) })));
        using var secondClient = new HttpClient(new StubHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(raw) })));
        var first = CreateExecutor(firstClient, "provider-a");
        var second = CreateExecutor(secondClient, "provider-b");
        var envelope = CreateEnvelope();

        var firstResult = await first.ExecuteAsync(envelope);
        var secondResult = await second.ExecuteAsync(envelope);

        Assert.Equal(firstResult.RequestFingerprint, secondResult.RequestFingerprint);
        Assert.NotEqual(firstResult.ProviderId, secondResult.ProviderId);
    }

    private sealed class FixtureAdapter(string id) : IHeadlessAiAdapter
    {
        public string Id { get; } = id;

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
                new Dictionary<string, string>
                {
                    ["response_id"] = "fixture-response",
                    ["input_tokens"] = "12",
                    ["output_tokens"] = "8"
                });
        }
    }

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }
}
