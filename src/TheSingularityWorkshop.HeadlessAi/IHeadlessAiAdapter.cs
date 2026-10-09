namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Maps normalized input to endpoint-specific HTTP and extracts normalized output.</summary>
public interface IHeadlessAiAdapter
{
    /// <summary>Stable identifier for this protocol adapter.</summary>
    string Id { get; }

    /// <summary>Create a request; the agent applies the profile endpoint, method, and default headers.</summary>
    HttpRequestMessage CreateRequest(HeadlessAiProfile profile, HeadlessAiInput input);

    /// <summary>Extract useful content from a successful response.</summary>
    ValueTask<HeadlessAiOutput> ReadResponseAsync(HttpResponseMessage response,
        CancellationToken cancellationToken = default);
}
