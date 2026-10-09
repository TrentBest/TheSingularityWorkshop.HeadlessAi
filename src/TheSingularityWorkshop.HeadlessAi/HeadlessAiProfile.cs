namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Describes an endpoint and the non-secret defaults used for invocation.</summary>
public sealed class HeadlessAiProfile
{
    public HeadlessAiProfile(string id, Uri endpoint, HttpMethod? method = null,
        IReadOnlyDictionary<string, string>? headers = null, TimeSpan? timeout = null,
        bool allowInsecureHttp = false)
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
        Id = id;
        Endpoint = endpoint;
        Method = method ?? HttpMethod.Post;
        Headers = headers is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
        Timeout = effectiveTimeout;
    }

    public string Id { get; }
    public Uri Endpoint { get; }
    public HttpMethod Method { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public TimeSpan Timeout { get; }
}
