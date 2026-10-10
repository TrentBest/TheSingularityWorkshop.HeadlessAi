using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// JSON-focused adapter that keeps provider-specific request shape and response extraction explicit
/// while handling UTF-8 JSON content and parsing centrally.
/// </summary>
public sealed class JsonHeadlessAiAdapter : IHeadlessAiAdapter
{
    private readonly Func<HeadlessAiProfile, HeadlessAiInput, JsonNode?> _requestFactory;
    private readonly Func<JsonElement, string> _responseSelector;
    private readonly Func<JsonElement, IReadOnlyDictionary<string, string>?>? _metadataSelector;

    /// <summary>Creates an adapter with endpoint-specific JSON mapping delegates.</summary>
    /// <param name="id">Stable adapter identifier.</param>
    /// <param name="requestFactory">Builds the exact JSON body required by the endpoint.</param>
    /// <param name="responseSelector">Extracts the normalized text from the parsed JSON response root.</param>
    /// <param name="metadataSelector">Optionally extracts provider usage or request metadata.</param>
    public JsonHeadlessAiAdapter(
        string id,
        Func<HeadlessAiProfile, HeadlessAiInput, JsonNode?> requestFactory,
        Func<JsonElement, string> responseSelector,
        Func<JsonElement, IReadOnlyDictionary<string, string>?>? metadataSelector = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
        _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
        _responseSelector = responseSelector ?? throw new ArgumentNullException(nameof(responseSelector));
        _metadataSelector = metadataSelector;
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public HttpRequestMessage CreateRequest(HeadlessAiProfile profile, HeadlessAiInput input)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(input);
        var body = _requestFactory(profile, input)
            ?? throw new InvalidOperationException($"JSON adapter '{Id}' produced a null request body.");
        var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(body));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
        return new HttpRequestMessage(profile.Method, profile.Endpoint) { Content = content };
    }

    /// <inheritdoc />
    public async ValueTask<HeadlessAiOutput> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        var content = _responseSelector(document.RootElement)
            ?? throw new InvalidOperationException($"JSON adapter '{Id}' returned null response content.");
        var metadata = _metadataSelector?.Invoke(document.RootElement);
        return new HeadlessAiOutput(content, response.StatusCode, response.Content.Headers.ContentType?.MediaType, metadata);
    }
}
