using System.Net;
using TheSingularityWorkshop.HeadlessAi;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiTests
{
    [Fact]
    public void HttpException_PreservesOriginalThreeArgumentConstructorForBinaryCompatibility()
    {
        var constructor = typeof(HeadlessAiHttpException).GetConstructor(
            new[] { typeof(string), typeof(HttpStatusCode), typeof(string) });

        Assert.NotNull(constructor);
    }

    [Fact]
    public void Profile_RequiresHttpsUnlessExplicitlyOverridden()
    {
        Assert.Throws<ArgumentException>(() => new HeadlessAiProfile("test", new Uri("http://example.test")));
        var local = new HeadlessAiProfile("local", new Uri("http://localhost:8080"), allowInsecureHttp: true);
        Assert.Equal("local", local.Id);
    }


    [Fact]
    public void Profile_CopiesProviderSettingsIntoReadOnlyConfiguration()
    {
        var settings = new Dictionary<string, string> { ["model"] = "model-a" };
        var profile = new HeadlessAiProfile("fixture", new Uri("https://example.test"), settings: settings);
        settings["model"] = "model-b";

        Assert.Equal("model-a", profile.Settings["model"]);
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, string>)profile.Settings)["model"] = "model-c");
    }

    [Fact]
    public void Profile_RejectsZeroTimeout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HeadlessAiProfile("test", new Uri("https://example.test"), timeout: TimeSpan.Zero));
    }


    [Fact]
    public void Template_CreatesMultipleAgentsFromOneConfiguration()
    {
        using var client = new HttpClient(new StubHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") })));
        var profile = new HeadlessAiProfile("shared-profile", new Uri("https://example.test"));
        var template = new HeadlessAiAgentTemplate(profile, new RawTextHeadlessAiAdapter());

        var first = template.CreateAgent(client);
        var second = template.CreateAgent(client);

        Assert.NotSame(first, second);
        Assert.Equal(first.ProfileId, second.ProfileId);
        Assert.Equal(first.AdapterId, second.AdapterId);
    }

    [Fact]
    public async Task Agent_UsesInjectedTransportAndNormalizesOutput()
    {
        using var client = new HttpClient(new StubHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://example.test/invoke", request.RequestUri!.ToString());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("reply") });
        }));
        var profile = new HeadlessAiProfile("fixture", new Uri("https://example.test/invoke"));
        var agent = new HeadlessAiAgent(client, profile, new RawTextHeadlessAiAdapter());
        var output = await agent.SendAsync(new HeadlessAiInput("prompt"));
        Assert.Equal("reply", output.Content);
        Assert.Equal(HttpStatusCode.OK, output.StatusCode);
    }

    [Fact]
    public async Task DelegateAdapter_AllowsEndpointSpecificRequestAndResponseMapping()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/provider", request.RequestUri!.ToString());
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("application/custom+json", request.Content!.Headers.ContentType!.MediaType);
            Assert.Equal("{\"task\":\"compress\"}", await request.Content.ReadAsStringAsync());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"result\":\"done\"}") };
        }));
        var adapter = new DelegateHeadlessAiAdapter(
            "fixture-json",
            (_, input) => new HttpRequestMessage(HttpMethod.Put, "https://ignored.invalid")
            {
                Content = new StringContent("{\"task\":\"" + input.Content + "\"}", System.Text.Encoding.UTF8, "application/custom+json")
            },
            async (response, token) =>
            {
                var body = await response.Content.ReadAsStringAsync(token);
                var value = System.Text.Json.JsonDocument.Parse(body).RootElement.GetProperty("result").GetString()!;
                return new HeadlessAiOutput(value, response.StatusCode, response.Content.Headers.ContentType?.MediaType);
            });
        var profile = new HeadlessAiProfile("fixture", new Uri("https://example.test/provider"), method: HttpMethod.Post);
        var agent = new HeadlessAiAgent(client, profile, adapter);

        var output = await agent.SendAsync(new HeadlessAiInput("compress"));

        Assert.Equal("done", output.Content);
    }


    [Fact]
    public async Task Agent_ResolvesRequestHeadersAtInvocationTime()
    {
        using var client = new HttpClient(new StubHandler((request, _) =>
        {
            Assert.Equal("rotated-token", request.Headers.GetValues("Authorization").Single());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
        }));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("fixture", new Uri("https://example.test"),
                headers: new Dictionary<string, string> { ["Authorization"] = "old-token" }),
            new RawTextHeadlessAiAdapter(),
            new FixedHeaderProvider());

        var output = await agent.SendAsync(new HeadlessAiInput("prompt"));

        Assert.Equal("ok", output.Content);
    }


    [Fact]
    public async Task Agent_ConvertsProfileTimeoutToTimeoutException()
    {
        using var client = new HttpClient(new StubHandler(async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("fixture", new Uri("https://example.test"), timeout: TimeSpan.FromMilliseconds(100)),
            new RawTextHeadlessAiAdapter());

        await Assert.ThrowsAsync<TimeoutException>(() => agent.SendAsync(new HeadlessAiInput("prompt")));
    }

    [Fact]
    public async Task Agent_DoesNotMisreportHttpClientTimeoutAsProfileTimeout()
    {
        using var client = new HttpClient(new StubHandler(async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }))
        {
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("fixture", new Uri("https://example.test"), timeout: TimeSpan.FromSeconds(5)),
            new RawTextHeadlessAiAdapter());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => agent.SendAsync(new HeadlessAiInput("prompt")));
    }

    [Fact]
    public async Task Agent_PreservesCallerCancellation()
    {
        using var client = new HttpClient(new StubHandler(async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("fixture", new Uri("https://example.test"), timeout: TimeSpan.FromSeconds(10)),
            new RawTextHeadlessAiAdapter());
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => agent.SendAsync(new HeadlessAiInput("prompt"), cancellation.Token));
    }

    [Fact]
    public async Task Agent_RejectsResponseBodiesOverProfileLimit()
    {
        using var client = new HttpClient(new StubHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("123456789")
            })));
        var profile = new HeadlessAiProfile("fixture", new Uri("https://example.test"), maxResponseBytes: 4);
        var agent = new HeadlessAiAgent(client, profile, new RawTextHeadlessAiAdapter());

        await Assert.ThrowsAsync<InvalidDataException>(() => agent.SendAsync(new HeadlessAiInput("prompt")));
    }

    [Fact]
    public async Task Agent_ReportsNonSuccessStatusAndBoundsTheResponseExcerpt()
    {
        using var client = new HttpClient(new StubHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent(new string('x', 3000)),
                Headers = { RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(30)) }
            })));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("fixture", new Uri("https://example.test/invoke")),
            new RawTextHeadlessAiAdapter());

        var error = await Assert.ThrowsAsync<HeadlessAiHttpException>(
            () => agent.SendAsync(new HeadlessAiInput("prompt")));

        Assert.Equal("fixture", error.ProfileId);
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(30), error.RetryAfter!.Delta);
        Assert.Equal(2048, error.ResponseExcerpt.Length);
        Assert.All(error.ResponseExcerpt, character => Assert.Equal('x', character));
    }

    private sealed class FixedHeaderProvider : IHeadlessAiRequestHeaderProvider
    {
        public ValueTask<IReadOnlyDictionary<string, string>> GetHeadersAsync(
            HeadlessAiProfile profile, CancellationToken cancellationToken = default)
            => ValueTask.FromResult<IReadOnlyDictionary<string, string>>(
                new Dictionary<string, string> { ["Authorization"] = "rotated-token" });
    }


    [Fact]
    public async Task JsonAdapter_MapsProviderSpecificRequestAndResponse()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/json", request.RequestUri!.ToString());
            Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);
            using var body = System.Text.Json.JsonDocument.Parse(await request.Content.ReadAsStringAsync());
            Assert.Equal("model-z", body.RootElement.GetProperty("model").GetString());
            Assert.Equal("task", body.RootElement.GetProperty("input").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"output\":\"mapped\",\"usage\":{\"total_tokens\":17}}",
                    System.Text.Encoding.UTF8, "application/json")
            };
        }));
        var adapter = new JsonHeadlessAiAdapter(
            "fixture-json",
            (profile, input) => new System.Text.Json.Nodes.JsonObject
            {
                ["model"] = profile.Settings["model"],
                ["input"] = input.Content
            },
            root => root.GetProperty("output").GetString()!,
            root => new Dictionary<string, string>
            {
                ["total_tokens"] = root.GetProperty("usage").GetProperty("total_tokens").GetInt32().ToString()
            });
        var profile = new HeadlessAiProfile("fixture", new Uri("https://example.test/json"),
            settings: new Dictionary<string, string> { ["model"] = "model-z" });
        var agent = new HeadlessAiAgent(client, profile, adapter);

        var output = await agent.SendAsync(new HeadlessAiInput("task"));

        Assert.Equal("mapped", output.Content);
        Assert.Equal("17", output.Metadata!["total_tokens"]);
    }

    [Fact]
    public async Task Agent_BoundsHttpErrorExcerpt()
    {
        using var client = new HttpClient(new StubHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(new string('x', 3000))
            })));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("fixture", new Uri("https://example.test")),
            new RawTextHeadlessAiAdapter());
        var exception = await Assert.ThrowsAsync<HeadlessAiHttpException>(
            () => agent.SendAsync(new HeadlessAiInput("prompt")));
        Assert.Equal(2048, exception.ResponseExcerpt.Length);
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Profile_RejectsNonPositiveResponseLimit(int maxResponseBytes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HeadlessAiProfile("test", new Uri("https://example.test"), maxResponseBytes: maxResponseBytes));
    }

    [Fact]
    public void Profile_RejectsRelativeEndpointUri()
    {
        var endpoint = new Uri("/invoke", UriKind.Relative);

        Assert.Throws<ArgumentException>(() => new HeadlessAiProfile("test", endpoint));
    }

    [Fact]
    public void Profile_AllowsExplicitInfiniteTimeout()
    {
        var profile = new HeadlessAiProfile(
            "test",
            new Uri("https://example.test"),
            timeout: System.Threading.Timeout.InfiniteTimeSpan);

        Assert.Equal(System.Threading.Timeout.InfiniteTimeSpan, profile.Timeout);
    }

    [Fact]
    public async Task Agent_DoesNotRetryNonSuccessResponsesAutomatically()
    {
        var attempts = 0;
        using var client = new HttpClient(new StubHandler((_, _) =>
        {
            attempts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("temporarily unavailable")
            });
        }));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("fixture", new Uri("https://example.test")),
            new RawTextHeadlessAiAdapter());

        await Assert.ThrowsAsync<HeadlessAiHttpException>(
            () => agent.SendAsync(new HeadlessAiInput("one invocation")));

        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task Agent_UsesProfileEndpointAndMethodEvenWhenAdapterSuggestsAnotherDestination()
    {
        using var client = new HttpClient(new StubHandler((request, _) =>
        {
            Assert.Equal("https://trusted.example.test/api", request.RequestUri!.ToString());
            Assert.Equal(HttpMethod.Put, request.Method);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("accepted")
            });
        }));
        var adapter = new DelegateHeadlessAiAdapter(
            "destination-fixture",
            (_, _) => new HttpRequestMessage(HttpMethod.Delete, "https://untrusted.invalid/steal"),
            (_, _) => ValueTask.FromResult(new HeadlessAiOutput("accepted", HttpStatusCode.OK)));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("trusted", new Uri("https://trusted.example.test/api"), method: HttpMethod.Put),
            adapter);

        var output = await agent.SendAsync(new HeadlessAiInput("request"));

        Assert.Equal("accepted", output.Content);
    }

    [Fact]
    public async Task OpenAiResponsesAdapter_UsesResponsesContractAndExtractsOutputText()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/v1/responses", request.RequestUri!.ToString());
            using var body = System.Text.Json.JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("fixture-model", body.RootElement.GetProperty("model").GetString());
            Assert.Equal("say hello", body.RootElement.GetProperty("input").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"id\":\"resp-1\",\"model\":\"fixture-model\",\"status\":\"completed\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"hello from responses\"}]}],\"usage\":{\"input_tokens\":2,\"output_tokens\":3,\"total_tokens\":5}}",
                    System.Text.Encoding.UTF8, "application/json")
            };
        }));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("openai-fixture", new Uri("https://example.test/v1/responses"),
                settings: new Dictionary<string, string> { ["model"] = "fixture-model" }),
            new OpenAiResponsesAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("say hello"));

        Assert.Equal("hello from responses", output.Content);
        Assert.Equal("resp-1", output.Metadata!["response_id"]);
        Assert.Equal("5", output.Metadata["total_tokens"]);
    }

    [Fact]
    public async Task GeminiGenerateContentAdapter_UsesGenerateContentContractAndExtractsCandidateText()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/v1beta/models/fixture:generateContent", request.RequestUri!.ToString());
            using var body = System.Text.Json.JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("say hello", body.RootElement.GetProperty("contents")[0].GetProperty("parts")[0].GetProperty("text").GetString());
            Assert.Equal(32, body.RootElement.GetProperty("generationConfig").GetProperty("maxOutputTokens").GetInt32());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"responseId\":\"gemini-1\",\"modelVersion\":\"fixture\",\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"hello from gemini\"}]}}],\"usageMetadata\":{\"totalTokenCount\":7}}",
                    System.Text.Encoding.UTF8, "application/json")
            };
        }));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("gemini-fixture", new Uri("https://example.test/v1beta/models/fixture:generateContent"),
                settings: new Dictionary<string, string> { ["max_output_tokens"] = "32" }),
            new GeminiGenerateContentAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("say hello"));

        Assert.Equal("hello from gemini", output.Content);
        Assert.Equal("gemini-1", output.Metadata!["response_id"]);
        Assert.Equal("7", output.Metadata["total_tokens"]);
    }

    [Fact]
    public async Task AnthropicMessagesAdapter_UsesMessagesContractAndExtractsTextBlocks()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/v1/messages", request.RequestUri!.ToString());
            using var body = System.Text.Json.JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("fixture-model", body.RootElement.GetProperty("model").GetString());
            Assert.Equal(64, body.RootElement.GetProperty("max_tokens").GetInt32());
            Assert.Equal("say hello", body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"id\":\"msg-1\",\"model\":\"fixture-model\",\"stop_reason\":\"end_turn\",\"content\":[{\"type\":\"text\",\"text\":\"hello from anthropic\"}],\"usage\":{\"input_tokens\":4,\"output_tokens\":6}}",
                    System.Text.Encoding.UTF8, "application/json")
            };
        }));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("anthropic-fixture", new Uri("https://example.test/v1/messages"),
                settings: new Dictionary<string, string> { ["model"] = "fixture-model", ["max_tokens"] = "64" }),
            new AnthropicMessagesAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("say hello"));

        Assert.Equal("hello from anthropic", output.Content);
        Assert.Equal("msg-1", output.Metadata!["message_id"]);
        Assert.Equal("6", output.Metadata["output_tokens"]);
    }

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }
}
