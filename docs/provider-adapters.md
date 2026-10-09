# Provider adapters

IHeadlessAiAdapter maps normalized input to an endpoint-specific HTTP request and extracts normalized output from a successful response. The profile provides endpoint, method, default headers, and timeout. The adapter owns provider-specific body shape and response parsing.

## Available adapter options

- RawTextHeadlessAiAdapter is a minimal UTF-8 text example, not a general LLM provider adapter.
- DelegateHeadlessAiAdapter lets a host provide request construction and response extraction delegates without adding a new class for every experiment.
- A dedicated adapter type is preferable when a provider protocol becomes stable, has multiple options, or needs extensive fixture tests.

The agent applies the profile endpoint and method after request construction, so the profile remains the authority for where the request is sent.

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

Open design questions include streaming, structured output, adapter version identity, provider error parsing, and request-time credentials. These should be driven by the first two actual endpoint integrations rather than speculation.
