# Roadmap

## Foundation
- [x] Establish .NET solution, library, tests, documentation, and initial transport slice.
- [x] Add provider-specific adapter seam and delegate-based adapter.
- [x] Add immutable provider settings, request-time header provider, timeout/cancellation, and response-size bounds.
- [ ] Verify the latest exact branch head in CI and fix all failures.
- [ ] Review API semantics before stabilizing the alpha surface.

## Adapter and transport hardening
- [ ] Add fixture-driven adapter for one real provider, using current official endpoint documentation.
- [ ] Add a second provider adapter to validate generality.
- [ ] Add explicit redirect/outbound-network policy and adapter parse-error taxonomy.
- [ ] Add safe diagnostics and correlation identifiers without content logging.
- [ ] Design retry policy with billable-operation/idempotency rules.

## Production readiness
- [ ] Explicit rate, concurrency, token, and cost budget boundaries.
- [ ] Streaming and structured-output contracts when justified by use cases.
- [ ] Benchmarks, compatibility checks, and package inspection.
- [ ] Optional host example composing ProtocolAi and GrammarAi.
- [ ] Reproducible measurements before claiming token or drift reductions.

Release gates: exact-head CI green, meaningful failure-path tests, reviewed package contents, documented limitations, and explicit approval before NuGet publication.
