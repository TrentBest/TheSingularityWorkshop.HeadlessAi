using System.Net;
using TheSingularityWorkshop.HeadlessAi;
using Xunit;

namespace TheSingularityWorkshop.HeadlessAi.Tests;

/// <summary>
/// Verifies that provider adapters fail explicitly instead of treating malformed or empty
/// provider payloads as successful, meaningful text.
/// </summary>
public sealed class ProviderAdapterFailureTests
{
    public static TheoryData<IHeadlessAiAdapter> ProviderAdapters => new()
    {
        new OpenAiResponsesAdapter(),
        new GeminiGenerateContentAdapter(),
        new AnthropicMessagesAdapter()
    };

    [Theory]
    [MemberData(nameof(ProviderAdapters))]
    public async Task ProviderAdapter_RejectsValidJsonWithoutText(IHeadlessAiAdapter adapter)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };

        await Assert.ThrowsAsync<InvalidDataException>(
            async () => await adapter.ReadResponseAsync(response));
    }

    [Theory]
    [MemberData(nameof(ProviderAdapters))]
    public async Task ProviderAdapter_RejectsMalformedJson(IHeadlessAiAdapter adapter)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ definitely not json")
        };

        await Assert.ThrowsAnyAsync<System.Text.Json.JsonException>(
            async () => await adapter.ReadResponseAsync(response));
    }

    [Theory]
    [MemberData(nameof(ProviderAdapters))]
    public async Task ProviderAdapter_RejectsEmptyResponseBody(IHeadlessAiAdapter adapter)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty)
        };

        await Assert.ThrowsAnyAsync<System.Text.Json.JsonException>(
            async () => await adapter.ReadResponseAsync(response));
    }

    [Theory]
    [InlineData("max_output_tokens", "not-an-integer")]
    [InlineData("temperature", "not-a-number")]
    [InlineData("store", "sometimes")]
    public void OpenAiAdapter_RejectsMalformedSettingsWithSettingName(string setting, string value)
    {
        var profile = new HeadlessAiProfile("fixture-profile", new Uri("https://example.com/v1/responses"),
            settings: new Dictionary<string, string> { ["model"] = "fixture-model", [setting] = value });

        var exception = Assert.Throws<ArgumentException>(() =>
            new OpenAiResponsesAdapter().CreateRequest(profile, new HeadlessAiInput("task")));

        Assert.Contains("fixture-profile", exception.Message);
        Assert.Contains(setting, exception.Message);
    }

    [Fact]
    public void AnthropicAdapter_RejectsMalformedRequiredTokenLimitWithSettingName()
    {
        var profile = new HeadlessAiProfile("fixture-profile", new Uri("https://example.com/v1/messages"),
            settings: new Dictionary<string, string> { ["model"] = "fixture-model", ["max_tokens"] = "many" });

        var exception = Assert.Throws<ArgumentException>(() =>
            new AnthropicMessagesAdapter().CreateRequest(profile, new HeadlessAiInput("task")));

        Assert.Contains("fixture-profile", exception.Message);
        Assert.Contains("max_tokens", exception.Message);
    }

}
