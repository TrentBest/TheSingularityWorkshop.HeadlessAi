# Security model

HeadlessAi sends data to endpoints configured by the host. Endpoint configuration, credentials, and response handling are security-sensitive.

- HTTPS is required by default. Plain HTTP requires explicit opt-in for trusted local/test endpoints.
- Prefer IHeadlessAiRequestHeaderProvider for request-time secret retrieval and token rotation. Static profile headers are copied into immutable collections, but their values may still be secrets.
- HeadlessAi does not log credentials, prompts, or responses.
- Successful response bodies are buffered only up to the profile's MaxResponseBytes limit.
- Non-success response excerpts are limited to 2048 bytes, but may still contain sensitive content; redact before persistence.
- Do not execute model-produced commands, code, URLs, tool calls, or configuration without a host-owned permission gate.
- Validate structured output before passing it to privileged actions.
- Server hosts should restrict outbound destinations and redirect behavior to mitigate SSRF and unsafe redirects.
- The host owns authorization, endpoint allowlists, tool policy, budgets, and audit.
- Provider-controlled content must never redefine credentials or authorization policy.

Before production, add a formal outbound-network policy, safe diagnostics, and carefully designed retry rules. Never blindly retry billable POST requests.
