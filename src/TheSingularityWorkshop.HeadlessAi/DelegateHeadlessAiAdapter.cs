namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Adapter that delegates endpoint-specific request construction and response extraction to host code.</summary>
public sealed class DelegateHeadlessAiAdapter : IHeadlessAiAdapter
{
    private readonly Func<HeadlessAiProfile, HeadlessAiInput, HttpRequestMessage> _requestFactory;
    private readonly Func<HttpResponseMessage, CancellationToken, ValueTask<HeadlessAiOutput>> _responseReader;

    /// <summary>Creates an adapter from explicit request and response delegates.</summary>
    /// <param name="id">Stable adapter identifier.</param>
    /// <param name="requestFactory">Creates endpoint-specific content and headers. The agent applies the profile URI and method.</param>
    /// <param name="responseReader">Extracts a normalized result from a successful HTTP response.</param>
    public DelegateHeadlessAiAdapter(
        string id,
        Func<HeadlessAiProfile, HeadlessAiInput, HttpRequestMessage> requestFactory,
        Func<HttpResponseMessage, CancellationToken, ValueTask<HeadlessAiOutput>> responseReader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
        _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
        _responseReader = responseReader ?? throw new ArgumentNullException(nameof(responseReader));
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public HttpRequestMessage CreateRequest(HeadlessAiProfile profile, HeadlessAiInput input)
        => _requestFactory(profile, input);

    /// <inheritdoc />
    public ValueTask<HeadlessAiOutput> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        => _responseReader(response, cancellationToken);
}
