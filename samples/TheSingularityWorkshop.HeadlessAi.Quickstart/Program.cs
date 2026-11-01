using TheSingularityWorkshop.HeadlessAi;

var profile = new HeadlessAiProfile(
    id: "research",
    endpoint: new Uri("https://api.openai.com/v1/responses"),
    settings: new Dictionary<string, string>
    {
        ["model"] = "your-model",
        ["instructions"] = "Answer in one concise paragraph.",
        ["max_output_tokens"] = "256"
    });

using var httpClient = new HttpClient();
var template = new HeadlessAiAgentTemplate(
    profile,
    new OpenAiResponsesAdapter(),
    headerProvider: new EnvironmentBearerHeaderProvider());

var agent = template.CreateAgent(httpClient);
var result = await agent.SendAsync(new HeadlessAiInput("Explain what an HTTP endpoint is."));
Console.WriteLine(result.Content);

sealed class EnvironmentBearerHeaderProvider : IHeadlessAiRequestHeaderProvider
{
    public ValueTask<IReadOnlyDictionary<string, string>> GetHeadersAsync(
        HeadlessAiProfile profile,
        CancellationToken cancellationToken = default)
    {
        var token = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Set OPENAI_API_KEY before running this example.");

        IReadOnlyDictionary<string, string> headers =
            new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" };
        return ValueTask.FromResult(headers);
    }
}
