using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Direct HTTP adapter for Anthropic's Messages API. Configure x-api-key and anthropic-version through profile or dynamic headers.
/// </summary>
public sealed class AnthropicMessagesAdapter : IHeadlessAiAdapter
{
    private readonly JsonHeadlessAiAdapter _inner = new(
        "anthropic-messages",
        BuildRequest,
        ExtractText,
        ExtractMetadata);

    /// <inheritdoc />
    public string Id => _inner.Id;

    /// <inheritdoc />
    public HttpRequestMessage CreateRequest(HeadlessAiProfile profile, HeadlessAiInput input)
        => _inner.CreateRequest(profile, input);

    /// <inheritdoc />
    public ValueTask<HeadlessAiOutput> ReadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        => _inner.ReadResponseAsync(response, cancellationToken);

    private static JsonNode BuildRequest(HeadlessAiProfile profile, HeadlessAiInput input)
    {
        var body = new JsonObject
        {
            ["model"] = JsonProviderAdapterHelpers.RequireSetting(profile, "model"),
            ["max_tokens"] = int.Parse(JsonProviderAdapterHelpers.RequireSetting(profile, "max_tokens"), System.Globalization.CultureInfo.InvariantCulture),
            ["messages"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = input.Content
                }
            }
        };
        JsonProviderAdapterHelpers.AddOptionalString(body, profile, "system");
        JsonProviderAdapterHelpers.AddOptionalDouble(body, profile, "temperature");
        JsonProviderAdapterHelpers.AddOptionalDouble(body, profile, "top_p");
        JsonProviderAdapterHelpers.AddOptionalInt(body, profile, "top_k");
        return body;
    }

    private static string ExtractText(JsonElement root)
    {
        if (root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            var text = new StringBuilder();
            foreach (var part in content.EnumerateArray())
            {
                if (JsonProviderAdapterHelpers.ReadString(part, "type") == "text" &&
                    part.TryGetProperty("text", out var value) && value.ValueKind == JsonValueKind.String)
                {
                    if (text.Length > 0)
                        text.AppendLine();
                    text.Append(value.GetString());
                }
            }
            if (text.Length > 0)
                return text.ToString();
        }
        throw new InvalidDataException("Anthropic Messages API returned no text content block.");
    }

    private static IReadOnlyDictionary<string, string>? ExtractMetadata(JsonElement root)
        => JsonProviderAdapterHelpers.CreateMetadata(
            ("message_id", JsonProviderAdapterHelpers.ReadString(root, "id")),
            ("model", JsonProviderAdapterHelpers.ReadString(root, "model")),
            ("stop_reason", JsonProviderAdapterHelpers.ReadString(root, "stop_reason")),
            ("input_tokens", JsonProviderAdapterHelpers.ReadUsage(root, "usage", "input_tokens")),
            ("output_tokens", JsonProviderAdapterHelpers.ReadUsage(root, "usage", "output_tokens")));
}
