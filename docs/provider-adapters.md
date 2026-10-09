# Provider adapters

IHeadlessAiAdapter maps normalized input to an endpoint-specific HTTP request and extracts normalized output from a successful response. The profile provides endpoint, method, default headers, timeout, response limit, and provider-specific non-secret settings.

## Available adapter options

- RawTextHeadlessAiAdapter is a minimal UTF-8 text example, not a general LLM provider adapter.
- DelegateHeadlessAiAdapter lets a host provide full request construction and response extraction delegates.
- JsonHeadlessAiAdapter handles UTF-8 JSON serialization/parsing while leaving the provider-specific request shape, response text extraction, and optional metadata extraction explicit.
- A dedicated adapter type is preferable when a provider protocol becomes stable, has multiple options, or needs extensive fixture tests.

The agent applies the profile endpoint and method after request construction, so the profile remains the authority for where the request is sent. JsonHeadlessAiAdapter is not a universal LLM schema; each endpoint still requires a correct request factory and response selector.

## Why provider-specific adapters?

Providers and model versions differ in message roles, system instructions, content blocks, model identifiers, generation options, tools, token accounting, and response envelopes. Pretending they share one schema creates hidden loss and brittle behavior.

## Adapter guidance

- Validate required profile settings before network I/O.
- Serialize once; avoid JSON-to-string-to-JSON cycles.
- Use versioned, scrubbed request and response fixtures.
- Test malformed JSON, missing fields, unexpected content types, and provider error envelopes.
- Preserve usage metadata and provider request IDs when available.
- Do not log authorization headers, prompts, or responses by default.
- Add a second real provider before generalizing fields common to only one provider.

Open design questions include streaming, structured output, adapter version identity, provider error parsing, and outbound-network policy. These should be driven by real endpoint integrations rather than speculation.
