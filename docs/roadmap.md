# Roadmap

## Release philosophy

HeadlessAi is intended to solve a real integration limitation, not to demonstrate that a tiny library can send one HTTP request. We should not artificially restrict the first release just because it is called an alpha. If a coherent part—or even most—of the useful HeadlessAi domain can be delivered, tested, and explained in the first release, we should do so.

The target is a **useful first release with honest boundaries**: implement the capabilities that belong together, test the public contract and failure paths, make common provider integrations understandable, and clearly label anything that is still provider-specific, experimental, or deferred. Domain coverage is an ambition; unsupported claims and untested behavior are not acceptable shortcuts.

## Foundation

- [x] Establish .NET solution, library, tests, documentation, and initial transport slice.
- [x] Add provider-specific adapter seam, delegate adapter, and JSON adapter.
- [x] Add immutable profiles, reusable agent templates, request-time headers, timeout/cancellation, and response-size bounds.
- [x] Add direct-HTTP adapters for OpenAI Responses, Gemini generateContent, and Anthropic Messages with fixture tests.
- [x] Add BenchmarkDotNet baseline, Codecov, package README metadata, and build/test/coverage/pack workflow.
- [x] Add an alpha readiness checklist that distinguishes passing CI, package inspection, release approval, and publication.

## Core usability and contract hardening

- [ ] Complete official-schema fixture coverage for provider success variants, error envelopes, and missing fields. Minimum malformed-JSON, empty-body, and valid-JSON-without-usable-text cases are now covered for all three built-in adapters; see [Provider Failure Contract](provider-failure-contract.md).
- [x] Document redirect behavior, credential-forwarding risk, and host-owned outbound endpoint validation; see [Security](security.md).
- [ ] Define a stable adapter parse-error taxonomy that callers can handle without depending on provider-specific parser internals.
- [ ] Add safe diagnostics and correlation identifiers without content or credential logging.
- [ ] Document and test configuration defaults, request headers, timeout/cancellation, response bounds, and error handling as one coherent developer journey.
- [ ] Add end-to-end fake-server examples showing the library from profile creation through normalized result and failure diagnosis.
- [ ] Verify package contents, XML docs, examples, provider setup instructions, and clean-install experience.

## Broader domain assessment

Review each capability by asking: Does it materially help a developer build a useful headless AI integration? Does it belong in this package, or should it remain an adapter/host concern? Can we define and test its contract now?

- [ ] Provider-neutral request/result contracts that preserve provider-specific information when normalization would lose meaning.
- [ ] Explicit capability declarations for text, streaming, multimodal input/output, structured output, and provider-native tools; do not imply every adapter supports every capability.
- [ ] Streaming support where it can be implemented with clear cancellation, disposal, error, and partial-result semantics.
- [ ] Provider-native tool/function-call representation without silently executing tools or granting authority to model output.
- [ ] Structured-output and schema-validation extension points, with no claim that a schema guarantees semantic correctness.
- [ ] Retry policy only after billable-operation, idempotency, cancellation, and partial-stream behavior are specified.
- [ ] Clear separation between library-level request controls and host-owned identity, authorization, concurrency, rate, token, and cost budgets.
- [ ] Extension guidance for custom providers/adapters and optional Forge integration using ProtocolAi/GrammarAi without making either a core dependency.

This is a discovery and implementation queue, not a promise that all items already exist. Prefer completing a capability coherently over adding a superficial API for it.

## Production readiness and measurement

- [ ] Benchmarks for adapter construction, JSON parsing, response size, and concurrent agent instances.
- [ ] Reproducible measurements of token use, accuracy, schema validity, hallucination/drift, latency, and cost before making comparative claims.
- [ ] Security review for endpoint configuration, redirects, secret handling, untrusted output, and host-owned authorization.
- [ ] Document supported .NET target, provider/API compatibility assumptions, and versioning policy.

## Alpha release gates

- [ ] CI passes on the exact candidate commit, including build, tests, coverage, and package packing.
- [ ] Review failure-path tests for profiles, transport, credentials, response bounds, and provider adapters.
- [ ] Inspect the generated package from a clean artifact.
- [ ] Confirm documentation distinguishes implemented behavior, provider-specific limitations, experimental work, and future plans.
- [ ] Confirm NuGet publication remains gated by `&& false` until explicit owner approval.

A passing build is necessary but not sufficient. Alpha readiness means a user can understand the intended contract, get a useful integration running without guesswork, recognize limitations, and report a reproducible issue. NuGet publication still requires explicit approval.
