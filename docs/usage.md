# Usage guide

Create a profile, inject a shared HttpClient, and bind an adapter:

    var profile = new HeadlessAiProfile(
        "my-endpoint",
        new Uri("https://example.invalid/generate"),
        method: HttpMethod.Post,
        headers: new Dictionary<string, string> { ["X-Example"] = "value" },
        timeout: TimeSpan.FromSeconds(20));

    using var httpClient = new HttpClient();
    var agent = new HeadlessAiAgent(httpClient, profile, new RawTextHeadlessAiAdapter());
    var result = await agent.SendAsync(new HeadlessAiInput("Task text"));

RawTextHeadlessAiAdapter is a minimal text example only. Many LLM endpoints require provider-specific JSON and nested response extraction. Implement IHeadlessAiAdapter for the exact endpoint contract; do not assume compatibility with GPT, Gemini, Claude, or any other provider without a matching adapter.

The host should use a secret provider rather than hardcoding credentials, and should own conversation state, roles, permissions, cost budgets, and tool mediation. Share HttpClient instances or use IHttpClientFactory; each HeadlessAiAgent is lightweight and does not own the transport.
