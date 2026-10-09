# Usage guide

A profile is a reusable endpoint configuration; an adapter defines that endpoint's request and response protocol; a template combines the two so a host can create many lightweight agent instances.

    var profile = new HeadlessAiProfile(
        "my-endpoint",
        new Uri("https://example.invalid/generate"),
        settings: new Dictionary<string, string>
        {
            ["model"] = "model-name",
            ["api-version"] = "endpoint-version"
        },
        timeout: TimeSpan.FromSeconds(20),
        maxResponseBytes: 1024 * 1024);

    using var httpClient = new HttpClient();
    var adapter = new RawTextHeadlessAiAdapter();
    var template = new HeadlessAiAgentTemplate(profile, adapter);
    var researcher = template.CreateAgent(httpClient);
    var reviewer = template.CreateAgent(httpClient);

For JSON endpoints, use JsonHeadlessAiAdapter with a request factory that constructs the exact body and a response selector that extracts the endpoint's useful field. It also supports an optional metadata selector for provider usage fields. DelegateHeadlessAiAdapter remains available when the endpoint needs custom HTTP behavior beyond JSON mapping.

RawTextHeadlessAiAdapter is a minimal text example only. Do not assume compatibility with GPT, Gemini, Claude, or any other provider without a matching request schema and response extraction contract.

## Credentials

Use IHeadlessAiRequestHeaderProvider to retrieve authorization headers at request time from a host-owned secret store or token refresh mechanism. Static profile headers are suitable for non-secret defaults; dynamic headers override same-named profile headers. HeadlessAi does not log header values. Never hardcode production credentials in source or checked-in configuration.

## Transport and limits

Both agents use the same immutable endpoint profile, adapter, and shared HTTP transport. They are separate caller instances, not separate conversations or permission contexts; the host owns conversation state, role policy, budgets, and tool mediation. Shared adapters and header providers should be stateless or thread-safe.

Share HttpClient instances or use IHttpClientFactory; each agent is lightweight and does not own the transport. The profile timeout applies in addition to the caller's cancellation token. Successful response bodies are bounded by MaxResponseBytes, and non-success excerpts are bounded to 2048 bytes.
