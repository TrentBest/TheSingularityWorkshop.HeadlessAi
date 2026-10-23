using System.Net;
using System.Text;
using TheSingularityWorkshop.HeadlessAi;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class CiAiCompatibilityTests
{
    [Fact]
    public async Task Agent_TransportsBoundedIntegerRequestAndLeavesResponseForHostValidation()
    {
        const string boundedRequest = "17\n2\n42";
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/llm", request.RequestUri!.AbsoluteUri);
            Assert.Equal(boundedRequest, await request.Content!.ReadAsStringAsync());
            Assert.Equal("text/plain", request.Content.Headers.ContentType!.MediaType);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("999", Encoding.UTF8, "text/plain")
            };
        }));
        var profile = new HeadlessAiProfile("fixture", new Uri("https://example.test/llm"));
        var agent = new HeadlessAiAgent(client, profile, new RawTextHeadlessAiAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput(boundedRequest));

        Assert.Equal("999", output.Content);
        var allowedIntentIds = new HashSet<int> { 1, 2, 3 };
        Assert.False(int.TryParse(output.Content, out var intentId) && allowedIntentIds.Contains(intentId));
    }

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request, cancellationToken);
    }
}
