using System.Net;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Represents a non-success HTTP response from a configured endpoint.</summary>
public sealed class HeadlessAiHttpException : HttpRequestException
{
    /// <summary>Creates an exception with the profile, HTTP status, and bounded response excerpt.</summary>
    /// <param name="profileId">Identifier of the endpoint profile that returned the error.</param>
    /// <param name="statusCode">HTTP response status code.</param>
    /// <param name="responseExcerpt">Bounded response text that may contain sensitive information.</param>
    public HeadlessAiHttpException(string profileId, HttpStatusCode statusCode, string responseExcerpt)
        : base($"HeadlessAi profile '{profileId}' returned HTTP {(int)statusCode} ({statusCode}).", null, statusCode)
    {
        ProfileId = profileId;
        ResponseExcerpt = responseExcerpt;
    }

    /// <summary>Gets the identifier of the profile that returned the HTTP error.</summary>
    public string ProfileId { get; }

    /// <summary>Gets bounded but potentially sensitive response text. Redact before persistence.</summary>
    public string ResponseExcerpt { get; }
}
