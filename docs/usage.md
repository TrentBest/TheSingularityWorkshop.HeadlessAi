# Usage guide

**The shortest path is: configure an endpoint, choose its adapter, create a caller, and send input.** This example uses the OpenAI Responses API and expects a real API key in the host environment. It makes a live network request; it is not an offline test.

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
```

Before running it, replace `your-model` with a model available to your account and set `OPENAI_API_KEY` in the process environment. Keep credentials out of source control.

A profile describes the endpoint and non-secret settings. An adapter maps the endpoint's request and response format. A template combines them, and `CreateAgent(httpClient)` creates a lightweight caller. Reuse the injected `HttpClient`; do not create a separate connection pool for every logical caller.

## Built-in direct-HTTP adapters

- OpenAiResponsesAdapter targets the OpenAI Responses API and reads model and optional generation settings from the profile.
- GeminiGenerateContentAdapter targets the exact model-specific Gemini generateContent URL and reads optional generation settings from the profile.
- AnthropicMessagesAdapter targets the Anthropic Messages endpoint and requires model and max_tokens settings.

All three use direct HTTP and System.Text.Json; no provider SDK is required. Configure each endpoint's required headers and settings as documented in [Provider adapters](provider-adapters.md). Direct HTTP does not bypass provider authentication, billing, rate limits, or access policy.

## Custom endpoint protocols

Use JsonHeadlessAiAdapter with a request factory that constructs the exact JSON body and a response selector that extracts the endpoint's useful field. It supports an optional metadata selector for provider usage fields. DelegateHeadlessAiAdapter remains available when an endpoint needs custom HTTP behavior beyond JSON mapping.

## Credentials and safety

Use IHeadlessAiRequestHeaderProvider to retrieve authorization headers at request time from a host-owned secret store or token refresh mechanism. Static profile headers are suitable for non-secret defaults; dynamic headers override same-named profile headers. HeadlessAi does not log header values. Never hardcode production credentials in source or checked-in configuration.

## Transport and limits

The profile timeout applies in addition to the caller's cancellation token. Successful response bodies are bounded by MaxResponseBytes, and non-success excerpts are bounded to 2048 bytes. The host owns conversation state, role policy, budgets, permissions, and tool mediation. Shared adapters and header providers should be stateless or thread-safe.
