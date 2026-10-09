using System.Text;

namespace TheSingularityWorkshop.HeadlessAi;

/// <summary>Minimal UTF-8 text adapter. Structured providers need their own protocol adapter.</summary>
public sealed class RawTextHeadlessAiAdapter : IHeadlessAiAdapter
{
    public string Id => "raw-text";

    public HttpRequestMessage CreateRequest(HeadlessAiProfile profile, HeadlessAiInput input)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(input);
        return new HttpRequestMessage(profile.Method, profile.Endpoint)
        {
            Content = new StringContent(input.Content, Encoding.UTF8, "text/plain")
        };
    }

    public async ValueTask<HeadlessAiOutput> ReadResponseAsync(HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return new HeadlessAiOutput(content, response.StatusCode, response.Content.Headers.ContentType?.MediaType);
    }
}
