namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>A reusable configured agent type that creates lightweight caller instances.</summary>
public sealed class HeadlessAiAgentTemplate
{
    /// <summary>Creates a template from an endpoint profile, protocol adapter, and optional request-time headers.</summary>
    /// <param name="profile">Reusable endpoint and non-secret provider configuration.</param>
    /// <param name="adapter">Endpoint-specific request and response mapping.</param>
    /// <param name="headerProvider">Optional request-time credential/header resolver.</param>
    public HeadlessAiAgentTemplate(HeadlessAiProfile profile, IHeadlessAiAdapter adapter,
        IHeadlessAiRequestHeaderProvider? headerProvider = null)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        HeaderProvider = headerProvider;
    }

    /// <summary>Gets the immutable profile shared by agents created from this template.</summary>
    public HeadlessAiProfile Profile { get; }

    /// <summary>Gets the adapter shared by agents created from this template.</summary>
    public IHeadlessAiAdapter Adapter { get; }

    /// <summary>Gets the optional request-time header provider shared by agents created from this template.</summary>
    public IHeadlessAiRequestHeaderProvider? HeaderProvider { get; }

    /// <summary>Creates an independent agent instance that uses the supplied shared transport.</summary>
    /// <param name="httpClient">Reusable HTTP transport owned by the host.</param>
    /// <returns>A new agent instance configured by this template.</returns>
    public HeadlessAiAgent CreateAgent(HttpClient httpClient)
        => new(httpClient, Profile, Adapter, HeaderProvider);
}
