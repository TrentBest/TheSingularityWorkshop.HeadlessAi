# Usage guide

Create a profile, inject a shared HttpClient, and bind an adapter:

    var profile = new HeadlessAiProfile(
        "my-endpoint",
        new Uri("https://example.invalid/generate"),
        method: HttpMethod.Post,
        settings: new Dictionary<string, string>
        {
            ["model"] = "model-name",
            ["api-version"] = "endpoint-version"
        },
        timeout: TimeSpan.FromSeconds(20),
        maxResponseBytes: 1024 * 1024);

    using var httpClient = new HttpClient();
    var agent = new HeadlessAiAgent(httpClient, profile, new RawTextHeadlessAiAdapter());
    var result = await agent.SendAsync(new HeadlessAiInput("Task text"));

Provider-specific settings are non-secret string values available to the adapter. The adapter decides how to map them and the normalized input to the exact endpoint request.

## Endpoint-specific behavior

RawTextHeadlessAiAdapter is a minimal text example only. For a JSON protocol, use DelegateHeadlessAiAdapter to provide request construction and response extraction, or implement IHeadlessAiAdapter as a dedicated adapter. Do not assume compatibility with GPT, Gemini, Claude, or any other provider without a matching adapter.

## Credentials

Use IHeadlessAiRequestHeaderProvider to retrieve authorization headers at request time from a host-owned secret store or token refresh mechanism. Static profile headers are suitable for non-secret defaults; dynamic headers override same-named profile headers. HeadlessAi does not log header values. Never hardcode production credentials in source or checked-in configuration.

## Transport and host responsibilities

Share HttpClient instances or use IHttpClientFactory; each agent is lightweight and does not own the transport. The profile timeout applies in addition to the caller's cancellation token. Successful response bodies are bounded by MaxResponseBytes. The host still owns conversation state, agent roles, permissions, cost budgets, and tool mediation.
