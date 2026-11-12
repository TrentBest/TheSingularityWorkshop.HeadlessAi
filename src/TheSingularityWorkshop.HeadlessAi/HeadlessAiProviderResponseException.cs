namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>
/// Represents a successful-HTTP response that a JSON provider adapter could not normalize.
/// The public error code is safe for classification; raw provider content remains internal because
/// it may contain credentials or other sensitive material.
/// </summary>
public sealed class HeadlessAiProviderResponseException : Exception
{
    internal HeadlessAiProviderResponseException(string errorCode, string responseBody)
        : base("The provider returned a response that could not be normalized.")
    {
        if (errorCode is not ("malformed_provider_json" or "invalid_provider_response"))
            throw new ArgumentOutOfRangeException(nameof(errorCode), "The provider response error code is not supported.");

        ErrorCode = errorCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Gets the stable provider-neutral failure code:
    /// <c>malformed_provider_json</c> or <c>invalid_provider_response</c>.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the already-buffered raw response body for same-assembly host normalization.
    /// This is intentionally not public because provider responses may contain sensitive data.
    /// Callers must screen it before persistence.
    /// </summary>
    internal string ResponseBody { get; }
}
