namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Translates normalized input into an endpoint-specific HTTP request and extracts a normalized response.
/// Implementations should be stateless or thread-safe because a template may share one adapter across concurrent agents.
/// Implementations must not log credentials or raw sensitive payloads.
/// </summary>
public interface IHeadlessAiAdapter
{
    /// <summary>Stable, human-readable identifier for this protocol adapter.</summary>
    string Id { get; }

    /// <summary>Create a request. The agent applies the profile endpoint and method, then applies profile and dynamic headers.</summary>
    HttpRequestMessage CreateRequest(HeadlessAiProfile profile, HeadlessAiInput input);

    /// <summary>Extract the useful response content. Called for successful HTTP responses only.</summary>
    ValueTask<HeadlessAiOutput> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default);
}
