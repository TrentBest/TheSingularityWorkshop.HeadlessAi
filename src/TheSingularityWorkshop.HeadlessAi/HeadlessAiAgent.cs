namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>A lightweight caller bound to one profile, adapter, and shared HTTP transport.</summary>
public sealed class HeadlessAiAgent
{
    private readonly HttpClient _httpClient;
    private readonly HeadlessAiProfile _profile;
    private readonly IHeadlessAiAdapter _adapter;

    /// <summary>Creates an agent that uses the supplied shared transport and endpoint protocol.</summary>
    /// <param name="httpClient">Reusable transport owned by the host, not by this agent.</param>
    /// <param name="profile">Endpoint settings for this agent.</param>
    /// <param name="adapter">Protocol-specific request and response mapping.</param>
    public HeadlessAiAgent(HttpClient httpClient, HeadlessAiProfile profile, IHeadlessAiAdapter adapter)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
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

        using var request = _adapter.CreateRequest(_profile, input)
            ?? throw new InvalidOperationException($"Adapter '{_adapter.Id}' returned a null request.");
        request.RequestUri = _profile.Endpoint;
        request.Method = _profile.Method;

        foreach (var header in _profile.Headers)
        {
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                if (request.Content is null || !request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    throw new InvalidOperationException($"Profile header '{header.Key}' could not be applied.");
            }
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                timeoutSource.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(timeoutSource.Token).ConfigureAwait(false);
                throw new HeadlessAiHttpException(_profile.Id, response.StatusCode,
                    body.Length <= 2048 ? body : body[..2048]);
            }
            return await _adapter.ReadResponseAsync(response, timeoutSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"The request for profile '{_profile.Id}' exceeded its configured timeout.");
        }
    }
}
