using System.Net;
using System.Net.Http.Headers;
using TheSingularityWorkshop.HeadlessAi;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiHttpExceptionTests
{
    [Fact]
    public void OriginalThreeArgumentConstructor_RemainsAvailable()
    {
        var constructor = typeof(HeadlessAiHttpException).GetConstructor(
            new[] { typeof(string), typeof(HttpStatusCode), typeof(string) });

        Assert.NotNull(constructor);
    }

    [Fact]
    public async Task Agent_PreservesRetryAfter_WithoutAutomaticallyRetrying()
    {
        var attempts = 0;
        using var client = new HttpClient(new StubHandler((_, _) =>
        {
            attempts++;
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("rate limited")
            };
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
            return Task.FromResult(response);
        }));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("retry-fixture", new Uri("https://example.test")),
            new RawTextHeadlessAiAdapter());

        var error = await Assert.ThrowsAsync<HeadlessAiHttpException>(
            () => agent.SendAsync(new HeadlessAiInput("one invocation")));

        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(30), error.RetryAfter!.Delta);
        Assert.Equal(1, attempts);
    }

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            send(request, cancellationToken);
    }
}
