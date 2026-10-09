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

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
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

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            async () => await adapter.ReadResponseAsync(response));
    }
}
