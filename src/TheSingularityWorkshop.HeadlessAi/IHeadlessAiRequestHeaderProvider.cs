namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Resolves request headers at invocation time, allowing host-managed secret retrieval and rotation.</summary>
public interface IHeadlessAiRequestHeaderProvider
{
    /// <summary>Gets headers to apply to a single request.</summary>
    /// <param name="profile">Endpoint profile being invoked.</param>
    /// <param name="cancellationToken">Token used to cancel credential resolution.</param>
    /// <returns>Request headers. Values are never logged by HeadlessAi.</returns>
    ValueTask<IReadOnlyDictionary<string, string>> GetHeadersAsync(
        HeadlessAiProfile profile,
        CancellationToken cancellationToken = default);
}
