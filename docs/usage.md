# Usage guide

A profile is a reusable endpoint configuration; an adapter defines that endpoint's request and response protocol; a template combines the two so a host can create many lightweight agent instances.

    var profile = new HeadlessAiProfile(
        "my-endpoint",
        new Uri("https://api.openai.com/v1/responses"),
        headers: new Dictionary<string, string> { ["Authorization"] = "Bearer supplied-by-secret-provider" },
        settings: new Dictionary<string, string>
        {
            ["model"] = "your-model",
            ["instructions"] = "Return concise, schema-conformant output.",
            ["max_output_tokens"] = "512"
        },
        timeout: TimeSpan.FromSeconds(30),
        maxResponseBytes: 1024 * 1024);

    using var httpClient = new HttpClient();
    var template = new HeadlessAiAgentTemplate(profile, new OpenAiResponsesAdapter());
    var researcher = template.CreateAgent(httpClient);
    var reviewer = template.CreateAgent(httpClient);

The profile and adapter are reusable; each agent is a lightweight caller instance. For rotating credentials, prefer IHeadlessAiRequestHeaderProvider instead of storing bearer tokens in static profile headers.

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
