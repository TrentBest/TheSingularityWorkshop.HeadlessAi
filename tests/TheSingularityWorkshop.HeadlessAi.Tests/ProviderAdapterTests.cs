using System.Net;
using System.Text.Json;
using TheSingularityWorkshop.HeadlessAi;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

public sealed class ProviderAdapterTests
{
    [Fact]
    public async Task OpenAiResponsesAdapter_MapsInputAndUsage()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("https://api.openai.com/v1/responses", request.RequestUri!.ToString());
            Assert.Equal("Bearer fixture", request.Headers.GetValues("Authorization").Single());
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("model-test", body.RootElement.GetProperty("model").GetString());
            Assert.Equal("task", body.RootElement.GetProperty("input").GetString());
            Assert.Equal("be concise", body.RootElement.GetProperty("instructions").GetString());
            Assert.Equal("resp_previous", body.RootElement.GetProperty("previous_response_id").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"id\":\"resp_current\",\"model\":\"model-test\",\"status\":\"completed\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"answer\"}]}],\"usage\":{\"input_tokens\":11,\"output_tokens\":4,\"total_tokens\":15}}",
                    System.Text.Encoding.UTF8, "application/json")
            };
        }));
        var profile = new HeadlessAiProfile("openai", new Uri("https://api.openai.com/v1/responses"),
            headers: new Dictionary<string, string> { ["Authorization"] = "Bearer fixture" },
            settings: new Dictionary<string, string>
            {
                ["model"] = "model-test",
                ["instructions"] = "be concise",
                ["max_output_tokens"] = "80",
                ["store"] = "false"
            });
        var agent = new HeadlessAiAgent(client, profile, new OpenAiResponsesAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("task",
            new Dictionary<string, string> { ["previous_response_id"] = "resp_previous" }));

        Assert.Equal("answer", output.Content);
        Assert.Equal("15", output.Metadata!["total_tokens"]);
    }

    [Fact]
    public async Task GeminiAdapter_MapsGenerationConfigAndUsage()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("x-goog-api-key", request.Headers.Single(h => h.Key == "x-goog-api-key").Key);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("task", body.RootElement.GetProperty("contents")[0].GetProperty("parts")[0].GetProperty("text").GetString());
            Assert.Equal("Use protocol", body.RootElement.GetProperty("system_instruction").GetProperty("parts")[0].GetProperty("text").GetString());
            Assert.Equal(64, body.RootElement.GetProperty("generationConfig").GetProperty("maxOutputTokens").GetInt32());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"responseId\":\"gemini-response\",\"modelVersion\":\"model-test\",\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"answer\"}]}}],\"usageMetadata\":{\"promptTokenCount\":9,\"candidatesTokenCount\":3,\"totalTokenCount\":12}}",
                    System.Text.Encoding.UTF8, "application/json")
            };
        }));
        var profile = new HeadlessAiProfile("gemini",
            new Uri("https://generativelanguage.googleapis.com/v1beta/models/model-test:generateContent"),
            headers: new Dictionary<string, string> { ["x-goog-api-key"] = "fixture" },
            settings: new Dictionary<string, string>
            {
                ["system_instruction"] = "Use protocol",
                ["max_output_tokens"] = "64",
                ["temperature"] = "0.2"
            });
        var agent = new HeadlessAiAgent(client, profile, new GeminiGenerateContentAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("task"));

        Assert.Equal("answer", output.Content);
        Assert.Equal("12", output.Metadata!["total_tokens"]);
    }

    [Fact]
    public async Task AnthropicAdapter_MapsMessagesAndUsage()
    {
        using var client = new HttpClient(new StubHandler(async (request, _) =>
        {
            Assert.Equal("fixture-key", request.Headers.GetValues("x-api-key").Single());
            Assert.Equal("2023-06-01", request.Headers.GetValues("anthropic-version").Single());
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("model-test", body.RootElement.GetProperty("model").GetString());
            Assert.Equal(100, body.RootElement.GetProperty("max_tokens").GetInt32());
            Assert.Equal("task", body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString());
            Assert.Equal("Use protocol", body.RootElement.GetProperty("system").GetString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"id\":\"msg_current\",\"model\":\"model-test\",\"stop_reason\":\"end_turn\",\"content\":[{\"type\":\"text\",\"text\":\"answer\"}],\"usage\":{\"input_tokens\":10,\"output_tokens\":5}}",
                    System.Text.Encoding.UTF8, "application/json")
            };
        }));
        var profile = new HeadlessAiProfile("anthropic", new Uri("https://api.anthropic.com/v1/messages"),
            headers: new Dictionary<string, string>
            {
                ["x-api-key"] = "fixture-key",
                ["anthropic-version"] = "2023-06-01"
            },
            settings: new Dictionary<string, string>
            {
                ["model"] = "model-test",
                ["max_tokens"] = "100",
                ["system"] = "Use protocol"
            });
        var agent = new HeadlessAiAgent(client, profile, new AnthropicMessagesAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("task"));

        Assert.Equal("answer", output.Content);
        Assert.Equal("5", output.Metadata!["output_tokens"]);
    }


    [Fact]
    public async Task OpenAiResponsesAdapter_CombinesTextPartsWithoutOptionalMetadata()
    {
        using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"first\"},{\"type\":\"output_text\",\"text\":\"second\"}]}]}",
                System.Text.Encoding.UTF8, "application/json")
        })));
        var profile = new HeadlessAiProfile("openai", new Uri("https://api.openai.com/v1/responses"),
            settings: new Dictionary<string, string> { ["model"] = "model-test" });
        var agent = new HeadlessAiAgent(client, profile, new OpenAiResponsesAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("task"));

        Assert.Equal($"first{Environment.NewLine}second", output.Content);
        Assert.Null(output.Metadata);
    }

    [Fact]
    public async Task GeminiAdapter_CombinesTextPartsWithoutOptionalMetadata()
    {
        using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"first\"},{\"text\":\"second\"}]}}]}",
                System.Text.Encoding.UTF8, "application/json")
        })));
        var profile = new HeadlessAiProfile("gemini",
            new Uri("https://generativelanguage.googleapis.com/v1beta/models/model-test:generateContent"));
        var agent = new HeadlessAiAgent(client, profile, new GeminiGenerateContentAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("task"));

        Assert.Equal($"first{Environment.NewLine}second", output.Content);
        Assert.Null(output.Metadata);
    }

    [Fact]
    public async Task AnthropicAdapter_CombinesTextBlocksWithoutOptionalMetadata()
    {
        using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"content\":[{\"type\":\"text\",\"text\":\"first\"},{\"type\":\"tool_use\",\"id\":\"tool-1\"},{\"type\":\"text\",\"text\":\"second\"}]}",
                System.Text.Encoding.UTF8, "application/json")
        })));
        var profile = new HeadlessAiProfile("anthropic", new Uri("https://api.anthropic.com/v1/messages"),
            settings: new Dictionary<string, string> { ["model"] = "model-test", ["max_tokens"] = "100" });
        var agent = new HeadlessAiAgent(client, profile, new AnthropicMessagesAdapter());

        var output = await agent.SendAsync(new HeadlessAiInput("task"));

        Assert.Equal($"first{Environment.NewLine}second", output.Content);
        Assert.Null(output.Metadata);
    }

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }
}
