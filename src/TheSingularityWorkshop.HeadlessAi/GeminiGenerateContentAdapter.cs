using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Direct HTTP adapter for Google's Gemini generateContent endpoint. Configure the exact model URL and credentials in the host.
/// </summary>
public sealed class GeminiGenerateContentAdapter : IHeadlessAiAdapter
{
    private readonly JsonHeadlessAiAdapter _inner = new(
        "gemini-generate-content",
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
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["parts"] = new JsonArray
                    {
                        new JsonObject { ["text"] = input.Content }
                    }
                }
            }
        };
        if (profile.Settings.TryGetValue("system_instruction", out var systemInstruction))
        {
            body["system_instruction"] = new JsonObject
            {
                ["parts"] = new JsonArray { new JsonObject { ["text"] = systemInstruction } }
            };
        }

        var generation = new JsonObject();
        JsonProviderAdapterHelpers.AddOptionalDouble(generation, profile, "temperature");
        JsonProviderAdapterHelpers.AddOptionalDouble(generation, profile, "top_p", "topP");
        JsonProviderAdapterHelpers.AddOptionalPositiveInt(generation, profile, "max_output_tokens", "maxOutputTokens");
        if (generation.Count > 0)
            body["generationConfig"] = generation;
        return body;
    }

    private static string ExtractText(JsonElement root)
    {
        if (root.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array)
        {
            var text = new StringBuilder();
            foreach (var candidate in candidates.EnumerateArray())
            {
                if (!candidate.TryGetProperty("content", out var content) ||
                    !content.TryGetProperty("parts", out var parts) || parts.ValueKind != JsonValueKind.Array)
                    continue;
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        if (text.Length > 0)
                            text.AppendLine();
                        text.Append(value.GetString());
                    }
                }
                if (text.Length > 0)
                    return text.ToString();
            }
        }
        var blockReason = JsonProviderAdapterHelpers.ReadString(root, "promptFeedback", "blockReason");
        throw new InvalidDataException(blockReason is null
            ? "Gemini generateContent returned no candidate text."
            : $"Gemini generateContent returned no candidate text (block reason: {blockReason}).");
    }

    private static IReadOnlyDictionary<string, string>? ExtractMetadata(JsonElement root)
        => JsonProviderAdapterHelpers.CreateMetadata(
            ("response_id", JsonProviderAdapterHelpers.ReadString(root, "responseId")),
            ("model", JsonProviderAdapterHelpers.ReadString(root, "modelVersion")),
            ("prompt_tokens", JsonProviderAdapterHelpers.ReadUsage(root, "usageMetadata", "promptTokenCount")),
            ("candidate_tokens", JsonProviderAdapterHelpers.ReadUsage(root, "usageMetadata", "candidatesTokenCount")),
            ("total_tokens", JsonProviderAdapterHelpers.ReadUsage(root, "usageMetadata", "totalTokenCount")));
}
