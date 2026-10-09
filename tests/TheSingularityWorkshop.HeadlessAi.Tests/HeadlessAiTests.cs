using System.Net;
using TheSingularityWorkshop.HeadlessAi;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiTests
{
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

    private sealed class FixedHeaderProvider : IHeadlessAiRequestHeaderProvider
    {
        public ValueTask<IReadOnlyDictionary<string, string>> GetHeadersAsync(
            HeadlessAiProfile profile, CancellationToken cancellationToken = default)
            => ValueTask.FromResult<IReadOnlyDictionary<string, string>>(
                new Dictionary<string, string> { ["Authorization"] = "rotated-token" });
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

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }
}
