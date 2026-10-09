using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Describes an endpoint and the non-secret defaults used for invocation.</summary>
public sealed class HeadlessAiProfile
{
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
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!endpoint.IsAbsoluteUri)
            throw new ArgumentException("The endpoint must be an absolute URI.", nameof(endpoint));
        if (endpoint.Scheme != Uri.UriSchemeHttps &&
            !(allowInsecureHttp && endpoint.Scheme == Uri.UriSchemeHttp))
            throw new ArgumentException("HTTPS is required unless insecure HTTP is explicitly enabled.", nameof(endpoint));
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(100);
        if (effectiveTimeout <= TimeSpan.Zero && effectiveTimeout != System.Threading.Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive or infinite.");
        if (maxResponseBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxResponseBytes), "Response size limit must be positive.");

        Id = id;
        Endpoint = endpoint;
        Method = method ?? HttpMethod.Post;
        Headers = CopyReadOnly(headers);
        Settings = CopyReadOnly(settings);
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

    private static IReadOnlyDictionary<string, string> CopyReadOnly(IReadOnlyDictionary<string, string>? source)
        => new ReadOnlyDictionary<string, string>(source is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase));
}
