namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>A lightweight caller bound to one profile, adapter, and shared HTTP transport.</summary>
public sealed class HeadlessAiAgent
{
    private readonly HttpClient _httpClient;
    private readonly HeadlessAiProfile _profile;
    private readonly IHeadlessAiAdapter _adapter;

    public HeadlessAiAgent(HttpClient httpClient, HeadlessAiProfile profile, IHeadlessAiAdapter adapter)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        ArgumentException.ThrowIfNullOrWhiteSpace(adapter.Id);
    }

    public string ProfileId => _profile.Id;
    public string AdapterId => _adapter.Id;

    /// <summary>Calls the endpoint without owning or disposing the injected HttpClient.</summary>
    public async Task<HeadlessAiOutput> SendAsync(HeadlessAiInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (_profile.Timeout != System.Threading.Timeout.InfiniteTimeSpan)
            timeoutSource.CancelAfter(_profile.Timeout);

        using var request = _adapter.CreateRequest(_profile, input)
            ?? throw new InvalidOperationException($"Adapter '{_adapter.Id}' returned a null request.");
        request.RequestUri ??= _profile.Endpoint;
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
