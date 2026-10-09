# Provider adapters

IHeadlessAiAdapter maps normalized input to an endpoint-specific HTTP request and extracts normalized output from a successful response. The profile provides endpoint, method, default headers, and timeout. The adapter owns provider-specific body shape and response parsing.

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
