using System.Text;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>A lightweight caller bound to one profile, adapter, and shared HTTP transport.</summary>
public sealed class HeadlessAiAgent
{
    private readonly HttpClient _httpClient;
    private readonly HeadlessAiProfile _profile;
    private readonly IHeadlessAiAdapter _adapter;
    private readonly IHeadlessAiRequestHeaderProvider? _headerProvider;

    /// <summary>Creates an agent that uses the supplied shared transport and endpoint protocol.</summary>
    /// <param name="httpClient">Reusable transport owned by the host, not by this agent.</param>
    /// <param name="profile">Endpoint settings for this agent.</param>
    /// <param name="adapter">Protocol-specific request and response mapping.</param>
    /// <param name="headerProvider">Optional request-time header source, commonly backed by a host secret provider.</param>
    public HeadlessAiAgent(HttpClient httpClient, HeadlessAiProfile profile, IHeadlessAiAdapter adapter,
        IHeadlessAiRequestHeaderProvider? headerProvider = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _headerProvider = headerProvider;
        ArgumentException.ThrowIfNullOrWhiteSpace(adapter.Id);
    }

    /// <summary>Gets the identifier of the endpoint profile used by this agent.</summary>
    public string ProfileId => _profile.Id;

    /// <summary>Gets the identifier of the protocol adapter used by this agent.</summary>
    public string AdapterId => _adapter.Id;

    /// <summary>Calls the configured endpoint without owning or disposing the injected HttpClient.</summary>
    /// <param name="input">Normalized input that the adapter maps to the endpoint protocol.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns>The adapter-normalized endpoint response.</returns>
    /// <exception cref="HeadlessAiHttpException">The endpoint returns a non-success HTTP status.</exception>
    /// <exception cref="TimeoutException">The configured profile timeout expires before caller cancellation.</exception>
    public async Task<HeadlessAiOutput> SendAsync(HeadlessAiInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (_profile.Timeout != System.Threading.Timeout.InfiniteTimeSpan)
            timeoutSource.CancelAfter(_profile.Timeout);

        try
        {
            using var request = _adapter.CreateRequest(_profile, input)
                ?? throw new InvalidOperationException($"Adapter '{_adapter.Id}' returned a null request.");
            request.RequestUri = _profile.Endpoint;
            request.Method = _profile.Method;
            ApplyHeaders(request, _profile.Headers);

            if (_headerProvider is not null)
            {
                var dynamicHeaders = await _headerProvider.GetHeadersAsync(_profile, timeoutSource.Token).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("The request header provider returned null.");
                ApplyHeaders(request, dynamicHeaders, replaceExisting: true);
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                timeoutSource.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var excerpt = await ReadBoundedExcerptAsync(response.Content, timeoutSource.Token).ConfigureAwait(false);
                throw new HeadlessAiHttpException(_profile.Id, response.StatusCode, excerpt);
            }

            await response.Content.LoadIntoBufferAsync(_profile.MaxResponseBytes, timeoutSource.Token).ConfigureAwait(false);
            return await _adapter.ReadResponseAsync(response, timeoutSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"The request for profile '{_profile.Id}' exceeded its configured timeout.");
        }
    }

    private static void ApplyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string> headers,
        bool replaceExisting = false)
    {
        foreach (var header in headers)
        {
            if (replaceExisting)
            {
                request.Headers.Remove(header.Key);
                request.Content?.Headers.Remove(header.Key);
            }

            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value) &&
                (request.Content is null || !request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value)))
                throw new InvalidOperationException($"Profile header '{header.Key}' could not be applied.");
        }
    }

    private static async Task<string> ReadBoundedExcerptAsync(HttpContent content, CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var buffer = new byte[2049];
        var total = 0;
        while (total < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(total, buffer.Length - total), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
                break;
            total += read;
        }
        return Encoding.UTF8.GetString(buffer, 0, Math.Min(total, 2048));
    }
}
