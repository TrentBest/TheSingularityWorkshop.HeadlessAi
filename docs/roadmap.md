# Roadmap

## Foundation
- [x] Establish .NET solution, library, tests, documentation, and initial transport slice.
- [x] Add provider-specific adapter seam, delegate adapter, and JSON adapter.
- [x] Add immutable profiles, reusable agent templates, request-time headers, timeout/cancellation, and response-size bounds.
- [x] Add direct-HTTP adapters for OpenAI Responses, Gemini generateContent, and Anthropic Messages with fixture tests.
- [x] Add BenchmarkDotNet baseline, Codecov, package README metadata, and build/test/coverage/pack workflow.
- [ ] Verify the latest exact branch head in CI and fix all failures.

## Adapter and transport hardening
- [ ] Add official-schema fixture tests for error envelopes and malformed responses for each provider.
- [ ] Add explicit redirect/outbound-network policy and adapter parse-error taxonomy.
- [ ] Add safe diagnostics and correlation identifiers without content logging.
- [ ] Design retry policy with billable-operation/idempotency rules.
- [ ] Design streaming support and richer multimodal inputs as explicit capabilities.

## Production readiness and measurement
- [ ] Explicit rate, concurrency, token, and cost budget boundaries at the correct host/library boundary.
- [ ] Benchmarks for adapter construction, JSON parsing, response size, and concurrent agent instances.
- [ ] Optional Forge integration example using ProtocolAi and GrammarAi without making either a core dependency.
- [ ] Reproducible measurements of token use, accuracy, schema validity, hallucination/drift, latency, and cost.

Release gates: exact-head CI green, meaningful failure-path tests, reviewed package contents, documented limitations, and explicit approval before NuGet publication.
