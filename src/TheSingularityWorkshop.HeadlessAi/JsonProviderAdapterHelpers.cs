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

    public static int RequireIntSetting(HeadlessAiProfile profile, string key)
        => ParseInt(profile, key, RequireSetting(profile, key));

    public static void AddOptionalString(JsonObject target, HeadlessAiProfile profile, string setting, string? property = null)
    {
        if (profile.Settings.TryGetValue(setting, out var value))
            target[property ?? setting] = value;
    }

    public static void AddOptionalInt(JsonObject target, HeadlessAiProfile profile, string setting, string? property = null)
    {
        if (profile.Settings.TryGetValue(setting, out var value))
            target[property ?? setting] = ParseInt(profile, setting, value);
    }

    public static void AddOptionalDouble(JsonObject target, HeadlessAiProfile profile, string setting, string? property = null)
    {
        if (profile.Settings.TryGetValue(setting, out var value))
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ||
                double.IsNaN(parsed) || double.IsInfinity(parsed))
                throw InvalidSetting(profile, setting, "a finite number");
            target[property ?? setting] = parsed;
        }
    }

    public static void AddOptionalBool(JsonObject target, HeadlessAiProfile profile, string setting, string? property = null)
    {
        if (profile.Settings.TryGetValue(setting, out var value))
        {
            if (!bool.TryParse(value, out var parsed))
                throw InvalidSetting(profile, setting, "true or false");
            target[property ?? setting] = parsed;
        }
    }

    private static int ParseInt(HeadlessAiProfile profile, string setting, string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            throw InvalidSetting(profile, setting, "an integer");
        return parsed;
    }

    private static ArgumentException InvalidSetting(HeadlessAiProfile profile, string setting, string expected)
        => new($"Profile '{profile.Id}' setting '{setting}' must be {expected}.", nameof(profile));

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
