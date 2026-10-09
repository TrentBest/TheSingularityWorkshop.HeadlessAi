# Roadmap

## Foundation
- [x] Establish .NET solution, library, tests, docs, and initial transport slice.
- [ ] Verify exact branch head in CI and fix all failures.
- [ ] Review API semantics before stabilizing the alpha surface.

## Adapter and transport hardening
- [ ] Request-time credential provider and rotation.
- [ ] Response-size limits and clearer adapter parse errors.
- [ ] Redacted diagnostics and correlation identifiers.
- [ ] Fixture-driven adapter for one real provider, using current official endpoint documentation.
- [ ] A second provider adapter to validate generality.
- [ ] Cancellation, timeout, content headers, malformed response, and redirect-policy tests.

## Production readiness
- [ ] Retry policy with billable-operation/idempotency rules.
- [ ] Explicit rate, concurrency, token, and cost budget boundaries.
- [ ] Streaming and structured-output contracts only when justified by use cases.
- [ ] Benchmarks, compatibility checks, and package inspection.
- [ ] Optional host example composing ProtocolAi and GrammarAi.
- [ ] Publish reproducible measurements before claiming token or drift reductions.

Release gates: exact-head CI green, meaningful failure-path tests, reviewed package contents, documented limitations, and explicit user approval before NuGet publication.
