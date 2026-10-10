using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Direct HTTP adapter for the OpenAI Responses API. The profile endpoint and credentials remain host-configured.
/// </summary>
public sealed class OpenAiResponsesAdapter : IHeadlessAiAdapter
{
    private readonly JsonHeadlessAiAdapter _inner = new(
        "openai-responses",
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
            ["input"] = input.Content
        };
        JsonProviderAdapterHelpers.AddOptionalString(body, profile, "instructions");
        JsonProviderAdapterHelpers.AddOptionalPositiveInt(body, profile, "max_output_tokens");
        JsonProviderAdapterHelpers.AddOptionalDouble(body, profile, "temperature");
        JsonProviderAdapterHelpers.AddOptionalDouble(body, profile, "top_p");
        JsonProviderAdapterHelpers.AddOptionalBool(body, profile, "store");
        if (input.Metadata?.TryGetValue("previous_response_id", out var previousResponseId) == true)
            body["previous_response_id"] = previousResponseId;
        return body;
    }

    private static string ExtractText(JsonElement root)
    {
        if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
        {
            var text = new StringBuilder();
            foreach (var item in output.EnumerateArray())
            {
                if (JsonProviderAdapterHelpers.ReadString(item, "type") != "message" ||
                    !item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                    continue;
                foreach (var part in content.EnumerateArray())
                {
                    if (JsonProviderAdapterHelpers.ReadString(part, "type") == "output_text" &&
                        part.TryGetProperty("text", out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        if (text.Length > 0)
                            text.AppendLine();
                        text.Append(value.GetString());
                    }
                }
            }
            if (text.Length > 0)
                return text.ToString();
        }
        throw new InvalidDataException("OpenAI Responses API returned no output_text content.");
    }

    private static IReadOnlyDictionary<string, string>? ExtractMetadata(JsonElement root)
        => JsonProviderAdapterHelpers.CreateMetadata(
            ("response_id", JsonProviderAdapterHelpers.ReadString(root, "id")),
            ("model", JsonProviderAdapterHelpers.ReadString(root, "model")),
            ("status", JsonProviderAdapterHelpers.ReadString(root, "status")),
            ("input_tokens", JsonProviderAdapterHelpers.ReadUsage(root, "usage", "input_tokens")),
            ("output_tokens", JsonProviderAdapterHelpers.ReadUsage(root, "usage", "output_tokens")),
            ("total_tokens", JsonProviderAdapterHelpers.ReadUsage(root, "usage", "total_tokens")));
}
