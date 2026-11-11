namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Represents a bounded successful-HTTP response that the configured JSON adapter cannot normalize.</summary>
internal sealed class HeadlessAiProviderResponseException : Exception
{
    public HeadlessAiProviderResponseException(string errorCode, string responseBody)
        : base("The provider returned a response that could not be normalized.")
    {
        ErrorCode = errorCode;
        ResponseBody = responseBody;
    }

    public string ErrorCode { get; }

    /// <summary>Gets the already-buffered response body. Callers must screen it before persistence.</summary>
    public string ResponseBody { get; }
}
