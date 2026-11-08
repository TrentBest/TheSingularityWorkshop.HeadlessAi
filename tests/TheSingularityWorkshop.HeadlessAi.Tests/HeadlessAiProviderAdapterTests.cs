using System.Net;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.HeadlessAi;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class HeadlessAiProviderAdapterTests
{
    [Fact]
    public async Task Agent_UsesProfileEndpointAndMethodInsteadOfAdapterSuggestedDestination()
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
            (_, _) => ValueTask.FromResult(new HeadlessAiOutput("accepted", HttpStatusCode.OK, "text/plain")));
        var agent = new HeadlessAiAgent(client,
            new HeadlessAiProfile("trusted", new Uri("https://trusted.example.test/api"), method: HttpMethod.Put),
            adapter);

        var output = await agent.SendAsync(new HeadlessAiInput("request"));

        Assert.Equal("accepted", output.Content);
    }

    [Fact]
    public async Task OpenAiResponsesAdapter_MapsRequestAndExtractsOutputText()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/v1/responses", request.RequestUri!.ToString());
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("fixture-model", body.RootElement.GetProperty("model").GetString());
            Assert.Equal("say hello", body.RootElement.GetProperty("input").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"id\":\"resp-1\",\"model\":\"fixture-model\",\"status\":\"completed\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"hello from responses\"}]}],\"usage\":{\"input_tokens\":2,\"output_tokens\":3,\"total_tokens\":5}}",
                    Encoding.UTF8, "application/json")
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
    public async Task GeminiGenerateContentAdapter_MapsRequestAndExtractsCandidateText()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/v1beta/models/fixture:generateContent", request.RequestUri!.ToString());
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("say hello", body.RootElement.GetProperty("contents")[0].GetProperty("parts")[0].GetProperty("text").GetString());
            Assert.Equal(32, body.RootElement.GetProperty("generationConfig").GetProperty("maxOutputTokens").GetInt32());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"responseId\":\"gemini-1\",\"modelVersion\":\"fixture\",\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"hello from gemini\"}]}}],\"usageMetadata\":{\"totalTokenCount\":7}}",
                    Encoding.UTF8, "application/json")
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
    public async Task AnthropicMessagesAdapter_MapsRequestAndExtractsTextBlocks()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://example.test/v1/messages", request.RequestUri!.ToString());
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("fixture-model", body.RootElement.GetProperty("model").GetString());
            Assert.Equal(64, body.RootElement.GetProperty("max_tokens").GetInt32());
            Assert.Equal("say hello", body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"id\":\"msg-1\",\"model\":\"fixture-model\",\"stop_reason\":\"end_turn\",\"content\":[{\"type\":\"text\",\"text\":\"hello from anthropic\"}],\"usage\":{\"input_tokens\":4,\"output_tokens\":6}}",
                    Encoding.UTF8, "application/json")
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
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            send(request, cancellationToken);
    }
}
