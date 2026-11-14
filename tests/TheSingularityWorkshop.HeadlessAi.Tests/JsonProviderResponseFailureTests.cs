using System.Net;
using System.Text;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class JsonProviderResponseFailureTests
{
    private static async Task<(HeadlessAiAgentTurnResult Result, int Calls)> ExecuteAsync(string responseBody)
    {
        var calls = 0;
        using var client = new HttpClient(new StubHandler((_, _) =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        }));
        var profile = new HeadlessAiProfile(
            "openai-test",
            new Uri("https://provider.example.test/v1/responses"),
            settings: new Dictionary<string, string> { ["model"] = "fixture-model" });
        var executor = new HeadlessAiAgentTurnExecutor(
            new HeadlessAiAgentTemplate(profile, new OpenAiResponsesAdapter()), client);
        var now = DateTimeOffset.UtcNow;
        var envelope = new HeadlessAiAgentTurnEnvelope
        {
            TaskId = "provider-response-test",
            CorrelationId = "provider-response-correlation",
            Repository = "TrentBest/HeadlessAi.Consumer",
            AuthorizedBranch = "agent/repair-ci",
            BaseCommitSha = "0123456789abcdef0123456789abcdef01234567",
            Role = "repair-agent",
            Instructions = "Diagnose the provider response.",
            CanonicalPrompt = "Return a structured proposal.",
            ExpiresAt = now.AddMinutes(10),
            Limits = new HeadlessAiAgentTurnLimits { Timeout = TimeSpan.FromSeconds(5) }
        };

        return (await executor.ExecuteAsync(envelope), calls);
    }

    [Fact]
    public async Task ExecuteAsync_MalformedProviderJsonIsNormalizedAndRetained()
    {
        const string raw = "{\"output\":";
        var (result, calls) = await ExecuteAsync(raw);

        Assert.Equal(1, calls);
        Assert.Equal("malformed_provider_json", result.ErrorCode);
        Assert.Equal(raw, result.RawResponse);
        Assert.Null(result.Proposal.PatchIntent);
        Assert.NotNull(result.Proposal.StopOrEscalateReason);
        Assert.Equal("The provider returned a response body that was not valid JSON.", result.Diagnostic);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_ProviderErrorJsonWithoutExpectedContentIsNotMisclassifiedAsSizeLimit()
    {
        const string raw = """{"error":{"message":"quota exceeded"}}""";
        var (result, calls) = await ExecuteAsync(raw);

        Assert.Equal(1, calls);
        Assert.Equal("invalid_provider_response", result.ErrorCode);
        Assert.Equal(raw, result.RawResponse);
        Assert.Null(result.Proposal.PatchIntent);
        Assert.NotNull(result.Proposal.StopOrEscalateReason);
        Assert.NotEqual("response_limit_exceeded", result.ErrorCode);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_JsonWithUnexpectedRootShapeIsNormalized()
    {
        const string raw = """["unexpected","array"]""";
        var (result, calls) = await ExecuteAsync(raw);

        Assert.Equal(1, calls);
        Assert.Equal("invalid_provider_response", result.ErrorCode);
        Assert.Equal(raw, result.RawResponse);
        Assert.Null(result.Proposal.PatchIntent);
        result.Validate();
    }

    [Fact]
    public async Task ExecuteAsync_MalformedProviderJsonContainingCredentialsIsNotRetained()
    {
        const string secret = "provider-secret-value-12345";
        const string raw = """{"authorization":"Bearer provider-secret-value-12345" """;
        var (result, calls) = await ExecuteAsync(raw);

        Assert.Equal(1, calls);
        Assert.Equal("malformed_provider_json", result.ErrorCode);
        Assert.Equal(string.Empty, result.RawResponse);
        Assert.DoesNotContain(secret, result.RawResponse);
        Assert.Null(result.Proposal.PatchIntent);
        result.Validate();
    }

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }
}
