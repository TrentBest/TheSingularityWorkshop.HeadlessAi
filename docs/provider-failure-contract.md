# Provider Failure Contract

A successful HTTP status is not proof that a provider returned a usable answer. HeadlessAi must distinguish transport success from a valid, meaningful provider result.

This document records the currently tested failure behavior. It is intentionally narrower than the full provider API specifications; each adapter's accepted success variants must continue to be checked against the provider's current official schema.

## Current adapter parsing behavior

The built-in OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters share these tested expectations:

| Response body | Expected behavior | Why |
|---|---|---|
| Valid JSON with the expected text structure | Extract the supported text and available metadata | Normalize only the fields the adapter actually understands. |
| Valid JSON such as `{}` with no usable text | Throw `InvalidDataException` | A structurally valid JSON document can still be an unusable provider response. |
| Malformed JSON | Throw a `System.Text.Json.JsonException` (including a derived reader exception) | Do not convert malformed payloads into successful empty content. |
| Empty body | Throw a `System.Text.Json.JsonException` | An empty body is not a valid JSON response. |

The malformed and empty-body response expectations are covered by parameterized tests in `ProviderAdapterFailureTests.cs`. The same test suite verifies actionable errors for malformed integer, number, and Boolean configuration values. These tests establish a common minimum, not exhaustive compatibility with every provider success variant, refusal, content-block type, error envelope, or API version.

## Invocation failures and host response

The broader invocation contract also distinguishes important operational cases:

- **Caller cancellation:** preserve the caller's cancellation signal; do not reinterpret it as a provider answer.
- **Profile timeout:** surface timeout as a timeout failure rather than successful empty output.
- **Non-success HTTP status:** surface `HeadlessAiHttpException` with a bounded response excerpt. The excerpt is limited to 2048 bytes and can still contain sensitive provider text; redact it before persistence.
- **Response too large:** enforce the configured `MaxResponseBytes` limit rather than buffering an unbounded successful body.
- **Invalid configuration:** reject unusable profile settings before relying on a remote endpoint. Missing required settings and malformed typed settings fail during request construction with an `ArgumentException` that identifies the profile and setting; malformed values are not sent to the provider.

Callers should handle failures at the host boundary, where they can decide whether to report, retry, abandon, or request human intervention. Do not automatically retry billable POST operations without a policy that accounts for idempotency, possible duplicate charges, cancellation, and partial results.

## Recommended host handling

1. Keep credentials and prompts out of ordinary logs.
2. Record a safe correlation identifier and coarse failure category where diagnostics are needed.
3. Preserve the distinction between caller cancellation and an endpoint timeout.
4. Treat provider output as untrusted data; validate required fields and semantics before consequential use.
5. Add fixture tests whenever an adapter changes its accepted request or response schema.
6. Verify fixtures against current official provider documentation instead of treating a sample payload as a complete schema.

## Deliberate gaps

The following remain release-hardening work rather than implied support: comprehensive official-schema fixtures for success variants and error envelopes; a stable public adapter parse-error taxonomy; safe correlation-aware diagnostics; and a fake-server example that exercises a complete invocation and failure diagnosis without a live provider.

See [Provider Adapters](provider-adapters.md), [Security Model](security.md), [Architecture Boundaries](architecture-boundaries.md), and the [Roadmap](roadmap.md).
