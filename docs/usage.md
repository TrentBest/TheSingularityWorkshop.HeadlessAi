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

Both agents use the same immutable endpoint profile, adapter, and shared HTTP transport. They are separate caller instances, not separate conversations or permission contexts; the host owns conversation state, role policy, budgets, and tool mediation. Shared adapters and header providers should be stateless or thread-safe.

## Endpoint-specific behavior

RawTextHeadlessAiAdapter is a minimal text example only. For a JSON protocol, use DelegateHeadlessAiAdapter to provide request construction and response extraction, or implement IHeadlessAiAdapter as a dedicated adapter. Provider-specific settings are non-secret strings available to the adapter. Do not assume compatibility with GPT, Gemini, Claude, or any other provider without a matching adapter.

## Credentials

Use IHeadlessAiRequestHeaderProvider to retrieve authorization headers at request time from a host-owned secret store or token refresh mechanism. Static profile headers are suitable for non-secret defaults; dynamic headers override same-named profile headers. HeadlessAi does not log header values. Never hardcode production credentials in source or checked-in configuration.

## Transport and limits

Share HttpClient instances or use IHttpClientFactory; each agent is lightweight and does not own the transport. The profile timeout applies in addition to the caller's cancellation token. Successful response bodies are bounded by MaxResponseBytes, and non-success excerpts are bounded to 2048 bytes.
