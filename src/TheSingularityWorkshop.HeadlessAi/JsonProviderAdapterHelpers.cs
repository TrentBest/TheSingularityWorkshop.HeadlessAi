using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheSingularityWorkshop.HeadlessAi;

internal static class JsonProviderAdapterHelpers
{
    public static string RequireSetting(HeadlessAiProfile profile, string key)
    {
        if (!profile.Settings.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Profile '{profile.Id}' requires the non-empty setting '{key}'.", nameof(profile));
        return value;
    }

    public static void AddOptionalString(JsonObject target, HeadlessAiProfile profile, string setting, string? property = null)
    {
        if (profile.Settings.TryGetValue(setting, out var value))
            target[property ?? setting] = value;
    }

    public static void AddOptionalInt(JsonObject target, HeadlessAiProfile profile, string setting, string? property = null)
    {
        if (profile.Settings.TryGetValue(setting, out var value))
            target[property ?? setting] = int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    public static void AddOptionalDouble(JsonObject target, HeadlessAiProfile profile, string setting, string? property = null)
    {
        if (profile.Settings.TryGetValue(setting, out var value))
            target[property ?? setting] = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    public static void AddOptionalBool(JsonObject target, HeadlessAiProfile profile, string setting, string? property = null)
    {
        if (profile.Settings.TryGetValue(setting, out var value))
            target[property ?? setting] = bool.Parse(value);
    }

    public static string? ReadString(JsonElement parent, string property)
        => parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(property, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()
            : null;

    public static string? ReadString(JsonElement parent, string objectProperty, string childProperty)
        => parent.ValueKind == JsonValueKind.Object &&
           parent.TryGetProperty(objectProperty, out var child) &&
           child.ValueKind == JsonValueKind.Object
            ? ReadString(child, childProperty)
            : null;

    public static IReadOnlyDictionary<string, string>? CreateMetadata(params (string Key, string? Value)[] values)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                result[key] = value;
        }
        return result.Count == 0 ? null : result;
    }

    public static string? ReadUsage(JsonElement root, string usageProperty, string field)
        => root.ValueKind == JsonValueKind.Object &&
           root.TryGetProperty(usageProperty, out var usage) &&
           usage.ValueKind == JsonValueKind.Object
            ? ReadString(usage, field)
            : null;
}
