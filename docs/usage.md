# Usage guide

**The basic operation is prompt in, response out—without a browser or GUI.** The host supplies input, HeadlessAi sends an HTTP request to a configured endpoint, and the host receives the returned content.

The current public API uses a `HeadlessAiProfile` and `HeadlessAiAgentTemplate` to configure that interaction. **A fluent endpoint builder is a design target, not part of the shipped API yet.** See the [fluent configuration design note](fluent-configuration-design.md) for the intended direction and its acceptance criteria.

This example uses **one specific protocol: OpenAI's Responses API**. That is an example choice, not a requirement or a generic endpoint. The URL, adapter, authentication headers, and settings must match the service you intend to call. This code calls the provider API directly; it does **not** automate the ChatGPT website or submit into the same signed-in web session. Provider APIs and their website experiences can have different features and conversation state.

The example expects a real API key in the host environment and makes a live network request; it is not an offline test.

## Current constructor-based API

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

In plain language, the profile records where and how to call; the adapter speaks the endpoint's request/response protocol; the template packages that reusable configuration; and the agent sends an individual request. Reuse the injected `HttpClient`; do not create a separate connection pool for every logical caller.

**Changing providers means changing the protocol configuration, not just swapping the URL.** Use a matching adapter when available, or provide a custom adapter for the endpoint's documented HTTP contract. HeadlessAi's core contracts do not need to know which model a service runs. A provider-specific adapter may need a model identifier as endpoint configuration, but that identifier remains data—not a HeadlessAi model type.

## Built-in direct-HTTP adapters

- `OpenAiResponsesAdapter` targets the OpenAI Responses API and reads model and optional generation settings from the profile.
- `GeminiGenerateContentAdapter` targets the exact model-specific Gemini generateContent URL and reads optional generation settings from the profile.
- `AnthropicMessagesAdapter` targets the Anthropic Messages endpoint and requires model and max_tokens settings.

All three use direct HTTP and System.Text.Json; no provider SDK is required. Configure each endpoint's required headers and settings as documented in [Provider adapters](provider-adapters.md). These are explicit protocol adapters—not evidence that the core API should depend on any provider's SDK or model classes. Direct HTTP does not bypass provider authentication, billing, rate limits, or access policy.

## Custom endpoint protocols

Use `JsonHeadlessAiAdapter` with a request factory that constructs the exact JSON body and a response selector that extracts the endpoint's useful field. It supports an optional metadata selector for provider usage fields. `DelegateHeadlessAiAdapter` remains available when an endpoint needs custom HTTP behavior beyond JSON mapping.

## Credentials and safety

Use `IHeadlessAiRequestHeaderProvider` to retrieve authorization headers at request time from a host-owned secret store or token refresh mechanism. Static profile headers are suitable for non-secret defaults; dynamic headers override same-named profile headers. HeadlessAi does not log header values. Never hardcode production credentials in source or checked-in configuration.

The host is responsible for validating outbound endpoint configuration for its deployment, including any server-side request forgery (SSRF) concerns. Review redirect behavior and credential forwarding when choosing endpoints and transport settings.

## Transport and limits

The profile timeout applies in addition to the caller's cancellation token. Successful response bodies are bounded by `MaxResponseBytes`, and non-success excerpts are bounded to 2048 bytes. The host owns conversation state, role policy, budgets, permissions, semantic validation, and tool mediation. Shared adapters and header providers should be stateless or thread-safe.

## Build and test

From the repository root:

```sh
dotnet restore TheSingularityWorkshop.HeadlessAi.slnx
dotnet build TheSingularityWorkshop.HeadlessAi.slnx --configuration Release --no-restore
dotnet test TheSingularityWorkshop.HeadlessAi.slnx --configuration Release --no-build
```

These commands verify the repository's code and tests; they do not validate a live provider credential or prove that a provider's current service accepts your account's configuration. Live examples require an authorized endpoint and credentials. Keep deterministic fixture tests as the normal way to test request mapping and response parsing.
