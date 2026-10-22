using System.Net;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Represents a non-success HTTP response from a configured endpoint.</summary>
public sealed class HeadlessAiHttpException : HttpRequestException
{
    /// <summary>Creates an exception with the profile, HTTP status, bounded response excerpt, and optional retry guidance.</summary>
    /// <param name="profileId">Identifier of the endpoint profile that returned the error.</param>
    /// <param name="statusCode">HTTP response status code.</param>
    /// <param name="responseExcerpt">Bounded response text that may contain sensitive information.</param>
    /// <param name="retryAfter">Optional Retry-After guidance supplied by the endpoint.</param>
    public HeadlessAiHttpException(string profileId, HttpStatusCode statusCode, string responseExcerpt,
        System.Net.Http.Headers.RetryConditionHeaderValue? retryAfter = null)
        : base($"HeadlessAi profile '{profileId}' returned HTTP {(int)statusCode} ({statusCode}).", null, statusCode)
    {
        ProfileId = profileId;
        ResponseExcerpt = responseExcerpt;
        RetryAfter = retryAfter;
    }

    /// <summary>Gets the identifier of the profile that returned the HTTP error.</summary>
    public string ProfileId { get; }

    /// <summary>Gets bounded but potentially sensitive response text. Redact before persistence.</summary>
    public string ResponseExcerpt { get; }

    /// <summary>Gets the server's optional Retry-After guidance for rate limits or temporary failures.</summary>
    /// <remarks>This is advisory metadata, not an automatic retry. The host must decide whether retrying is safe.</remarks>
    public System.Net.Http.Headers.RetryConditionHeaderValue? RetryAfter { get; }
}
