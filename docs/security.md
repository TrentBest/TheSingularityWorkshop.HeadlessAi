# Security model

HeadlessAi sends data to endpoints configured by the host. Endpoint configuration, credentials, and response handling are security-sensitive.

- HTTPS is required by default. Plain HTTP requires explicit opt-in for trusted local/test endpoints.
- Prefer `IHeadlessAiRequestHeaderProvider` for request-time secret retrieval and token rotation. Static profile headers are copied into immutable collections, but their values may still be secrets.
- HeadlessAi does not log credentials, prompts, or responses.
- Successful response bodies are buffered only up to the profile's `MaxResponseBytes` limit.
- Non-success response excerpts are limited to 2048 bytes, but may still contain sensitive content; redact before persistence.
- Do not execute model-produced commands, code, URLs, tool calls, or configuration without a host-owned permission gate.
- Validate structured output before passing it to privileged actions.
- The host owns authorization, endpoint allowlists, tool policy, budgets, and audit.
- Provider-controlled content must never redefine credentials or authorization policy.

## Redirects and outbound destinations

**The host supplies and owns `HttpClient`; HeadlessAi does not control the handler's redirect policy.** The default HTTP handler may follow redirects automatically. Because provider credentials can be sent in custom headers such as `x-api-key`, do not assume that automatic redirects are safe for every endpoint or handler configuration.

For server-side or credential-bearing use, prefer a transport that does not follow redirects automatically:

```csharp
using System.Net;

var handler = new SocketsHttpHandler
{
    AllowAutoRedirect = false
};

var httpClient = new HttpClient(handler);
```

When redirects are disabled, a 3xx response is treated as a non-success HTTP status and surfaced as `HeadlessAiHttpException`; the host can then decide whether the destination is allowed before making another request. If the application needs to follow redirects, it must validate each destination and decide explicitly whether credentials may be forwarded. Do not blindly replay credential-bearing requests to a new host.

Disabling redirects is not a complete SSRF defense. Server hosts should also enforce an outbound destination policy, account for DNS/IP resolution and private or link-local addresses, and prevent untrusted input from selecting arbitrary endpoints. The exact policy depends on the host's threat model.

## Production follow-up

Before production, add a formal outbound-network policy and safe diagnostics, and define carefully designed retry rules. Never blindly retry billable POST requests. A future HeadlessAi redirect abstraction should preserve the host's authority over transport and credentials rather than silently taking over its `HttpClient`.
