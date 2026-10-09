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
    public void Profile_RejectsZeroTimeout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HeadlessAiProfile("test", new Uri("https://example.test"), timeout: TimeSpan.Zero));
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
