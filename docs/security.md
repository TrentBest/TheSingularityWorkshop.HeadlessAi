# Security model

HeadlessAi sends data to endpoints configured by the host. Endpoint configuration, credentials, and response handling are security-sensitive.

- HTTPS is required by default. Plain HTTP requires explicit opt-in for trusted local/test endpoints.
- Never check credentials into source or include them in diagnostics.
- Treat prompts, responses, and provider error bodies as sensitive.
- Error excerpts are bounded but may still contain sensitive content; redact before persistence.
- Do not execute model-produced commands, code, URLs, tool calls, or configuration without a host-owned permission gate.
- Validate structured output before passing it to privileged actions.
- Server hosts should restrict outbound destinations to mitigate SSRF and unsafe redirects.
- The host owns authorization, endpoint allowlists, tool policy, budgets, and audit.
- Provider-controlled content must never redefine credentials or authorization policy.

Before production, add request-time credential resolution, explicit redirect/outbound-network policy, response-size limits, safe diagnostics, and carefully designed retry rules. Never blindly retry billable POST requests.
