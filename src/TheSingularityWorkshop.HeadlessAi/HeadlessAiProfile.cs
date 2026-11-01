using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Describes an endpoint and the non-secret defaults used for invocation.</summary>
public sealed class HeadlessAiProfile
{
    /// <summary>Maximum profile identifier length, in UTF-16 characters.</summary>
    public const int MaxIdCharacters = 128;
    /// <summary>Maximum absolute endpoint URI length, in UTF-16 characters.</summary>
    public const int MaxEndpointCharacters = 2_048;
    /// <summary>Maximum number of configured headers.</summary>
    public const int MaxHeaders = 64;
    /// <summary>Maximum number of provider settings.</summary>
    public const int MaxSettings = 128;
    /// <summary>Maximum header or setting key length, in UTF-16 characters.</summary>
    public const int MaxConfigurationKeyCharacters = 256;
    /// <summary>Maximum header or setting value length, in UTF-16 characters.</summary>
    public const int MaxConfigurationValueCharacters = 8_192;
    /// <summary>Maximum buffered response body size, in bytes.</summary>
    public const int MaxResponseBytesLimit = 64 * 1024 * 1024;
    /// <summary>Creates validated settings for one endpoint profile.</summary>
    /// <param name="id">Stable profile identifier.</param>
    /// <param name="endpoint">Absolute endpoint URI. HTTPS is required by default.</param>
    /// <param name="method">HTTP method, defaulting to POST.</param>
    /// <param name="headers">Default request headers. Treat values as sensitive if they contain credentials.</param>
    /// <param name="timeout">Request timeout; null selects 100 seconds and infinite is allowed explicitly.</param>
    /// <param name="allowInsecureHttp">Explicitly permits HTTP, intended for trusted local/test endpoints.</param>
    /// <param name="settings">Provider-specific non-secret string settings exposed to the adapter.</param>
    /// <param name="maxResponseBytes">Maximum buffered response body size in bytes.</param>
    public HeadlessAiProfile(string id, Uri endpoint, HttpMethod? method = null,
        IReadOnlyDictionary<string, string>? headers = null, TimeSpan? timeout = null,
        bool allowInsecureHttp = false, IReadOnlyDictionary<string, string>? settings = null,
        int maxResponseBytes = 4 * 1024 * 1024)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (id.Length > MaxIdCharacters)
            throw new ArgumentException($"Profile IDs cannot exceed {MaxIdCharacters} characters.", nameof(id));
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!endpoint.IsAbsoluteUri)
            throw new ArgumentException("The endpoint must be an absolute URI.", nameof(endpoint));
        if (endpoint.AbsoluteUri.Length > MaxEndpointCharacters)
            throw new ArgumentException($"Endpoint URIs cannot exceed {MaxEndpointCharacters} characters.", nameof(endpoint));
        if (endpoint.Scheme != Uri.UriSchemeHttps &&
            !(allowInsecureHttp && endpoint.Scheme == Uri.UriSchemeHttp))
            throw new ArgumentException("HTTPS is required unless insecure HTTP is explicitly enabled.", nameof(endpoint));
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(100);
        if (effectiveTimeout <= TimeSpan.Zero && effectiveTimeout != System.Threading.Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive or infinite.");
        if (maxResponseBytes <= 0 || maxResponseBytes > MaxResponseBytesLimit)
            throw new ArgumentOutOfRangeException(nameof(maxResponseBytes), $"Response size limit must be between 1 and {MaxResponseBytesLimit} bytes.");

        Id = id;
        Endpoint = endpoint;
        Method = method ?? HttpMethod.Post;
        Headers = CopyReadOnly(headers, MaxHeaders, nameof(headers));
        Settings = CopyReadOnly(settings, MaxSettings, nameof(settings));
        Timeout = effectiveTimeout;
        MaxResponseBytes = maxResponseBytes;
    }

    /// <summary>Gets the stable profile identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the absolute endpoint URI.</summary>
    public Uri Endpoint { get; }

    /// <summary>Gets the HTTP method used for requests.</summary>
    public HttpMethod Method { get; }

    /// <summary>Gets immutable profile default headers. Prefer request-time header providers for rotating secrets.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Gets immutable, provider-specific non-secret settings for adapter use.</summary>
    public IReadOnlyDictionary<string, string> Settings { get; }

    /// <summary>Gets the timeout for each request, or infinite when explicitly configured.</summary>
    public TimeSpan Timeout { get; }

    /// <summary>Gets the maximum buffered response body size in bytes.</summary>
    public int MaxResponseBytes { get; }

    private static IReadOnlyDictionary<string, string> CopyReadOnly(
        IReadOnlyDictionary<string, string>? source, int maxEntries, string parameterName)
    {
        var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (source is not null)
        {
            foreach (var pair in source)
            {
                if (copy.Count >= maxEntries)
                    throw new ArgumentException($"Configuration cannot contain more than {maxEntries} entries.", parameterName);
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > MaxConfigurationKeyCharacters)
                    throw new ArgumentException($"Configuration keys must contain 1–{MaxConfigurationKeyCharacters} non-whitespace characters.", parameterName);
                if (pair.Value is null || pair.Value.Length > MaxConfigurationValueCharacters)
                    throw new ArgumentException($"Configuration values cannot exceed {MaxConfigurationValueCharacters} characters or be null.", parameterName);
                if (!copy.TryAdd(pair.Key, pair.Value))
                    throw new ArgumentException($"Configuration contains duplicate key '{pair.Key}' (case-insensitive).", parameterName);
            }
        }

        return new ReadOnlyDictionary<string, string>(copy);
    }
}
