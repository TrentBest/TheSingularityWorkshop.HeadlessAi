namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Represents a non-success HTTP response from a configured endpoint.</summary>
public sealed class HeadlessAiHttpException : HttpRequestException
{
    public HeadlessAiHttpException(string profileId, HttpStatusCode statusCode, string responseExcerpt)
        : base($"HeadlessAi profile '{profileId}' returned HTTP {(int)statusCode} ({statusCode}).")
    {
        ProfileId = profileId;
        StatusCode = statusCode;
        ResponseExcerpt = responseExcerpt;
    }

    public string ProfileId { get; }
    public HttpStatusCode StatusCode { get; }
    /// <summary>Bounded but potentially sensitive response text. Redact before persistence.</summary>
    public string ResponseExcerpt { get; }
}
