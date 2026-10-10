# Alpha Readiness — HeadlessAi

> **Purpose:** prepare a first release that is genuinely useful, understandable, and testable. “Alpha” describes maturity and the need for real-world feedback; it is not a reason to artificially withhold coherent functionality.

## Latest candidate status — 2026-10-10 UTC

**Current development candidate:** `a05f19a4d24d9ab22228a482df5f440d61ae8758`

- [x] GitHub Actions run [#218](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/38017640412) completed successfully for this exact commit; the equivalent push run [#217](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/38017637532) also passed.
- [x] Restore and Release build passed with warnings treated as errors.
- [x] Automated tests and coverage report passed; Codecov upload succeeded. CI uses deterministic tests and does not require paid live-provider calls.
- [x] NuGet package was packed and audited by the workflow, then uploaded as the `headlessai-package` artifact (26,166 bytes; artifact retained at time of review).
- [x] NuGet publication step was skipped; publishing remains disabled by the workflow's `&& false` gate.
- [x] Additional boundary tests now reject invalid response-size limits, relative endpoint URIs, and zero/negative provider token limits; explicitly infinite timeout remains supported.
- [ ] Human release review is still required. CI passing does not itself establish release approval or complete every documentation/security/contract review.

**Assessment:** the technical alpha candidate is getting close: the exact current HEAD has a clean CI/package pass. Remaining work is a focused final review of the user-facing docs and security boundaries, the open development-to-master PR, and any checklist items not evidenced by automated checks. Do not hold the core alpha for streaming, multimodal support, orchestration, or the optional ProtocolAi/GrammarAi bridge.

## Release ambition

HeadlessAi addresses a major integration limitation: developers should be able to add AI capabilities to applications and services without adopting a heavyweight framework, GUI, application runtime, or single-provider architecture. The first release should deliver as much of that useful domain as can be implemented coherently and supported by tests and documentation.

The standard is **broad usefulness with honest boundaries**:

- Prefer a coherent end-to-end capability over a shallow collection of APIs.
- Keep the core independently usable and provider-extensible.
- Make provider differences explicit instead of pretending they are identical.
- Test normal operation and failure behavior using deterministic fixtures; do not make CI depend on paid live endpoints.
- Describe what is implemented today separately from experimental features, known limitations, and roadmap ambitions.
- Do not promise security, semantic correctness, performance, token savings, accuracy, or cost savings without evidence.

## What exists in the current foundation

The current implementation includes:

- Reusable endpoint profiles and agent templates.
- A lightweight invocation surface using a host-injected `HttpClient`.
- Raw-text, delegate-based, JSON, OpenAI Responses, Gemini `generateContent`, and Anthropic Messages adapters.
- Request-time header providers, cancellation, timeouts, and bounded response bodies.
- Normalized content and provider usage metadata where the response supplies it.
- Deterministic fixture tests, a BenchmarkDotNet baseline project, coverage reporting, and package packing in CI.

The built-in provider adapters are currently text-focused and non-streaming. The list above describes the present foundation, not a claim that the whole useful HeadlessAi domain is finished.

## Domain-completeness questions

Before treating the first release as ready, assess whether a developer can:

1. **Connect:** configure an endpoint, authentication headers, provider adapter, and request without guessing.
2. **Use:** send a request and consume a clear result while retaining important provider-specific details.
3. **Recover:** distinguish configuration, transport, HTTP, cancellation/timeout, response-size, and provider-payload failures.
4. **Extend:** add an adapter or customize request/response behavior without forking the package.
5. **Control:** understand which controls are provided by HeadlessAi and which remain the host application's responsibility.
6. **Secure:** handle credentials, outbound endpoints, untrusted output, and authorization boundaries intentionally.
7. **Learn:** follow a working example, understand the architecture, and find provider-specific setup and troubleshooting guidance.

If a missing capability materially prevents these outcomes, evaluate implementing it before the first release rather than deferring it solely because the package is labelled alpha. If a capability is too broad to complete safely, define its boundary and document it explicitly.

## Release gates

Use this checklist against the exact candidate commit. Do not infer readiness from an older successful run.

### 1. Build and package

- [x] Restore and Release build succeed with warnings treated as errors (run #218).
- [x] Unit tests pass without live provider calls or credentials (run #218).
- [x] Coverage report is produced and uploaded successfully (run #218).
- [x] Package is packed and the workflow's packed-package audit succeeds (run #218).
- [x] CI audit checks the assembly, XML docs, README, license, package ID/version, README portability, architecture visual, and internal README links inside the packed `.nupkg`; manually confirm any packaging details not covered by the audit.
- [x] Candidate SHA recorded and package artifact retained: `a05f19a4d24d9ab22228a482df5f440d61ae8758`, artifact `headlessai-package` from run #218.

### 2. Contract and failure-path tests

- [ ] Profile validation covers absolute URI, HTTPS-by-default, explicit local HTTP opt-in, positive response limit, and timeout rules.
- [ ] Transport tests cover caller cancellation, configured timeout, non-success HTTP status, oversized success body, and bounded error excerpts.
- [ ] Header tests cover request-time resolution and dynamic override behavior without leaking values.
- [ ] Adapter tests cover valid response variants, missing/malformed required fields, empty text, provider error responses, and usage metadata when present.
- [ ] Tests use deterministic fixtures and fake HTTP handlers; CI does not depend on paid endpoints.
- [ ] Any newly added capability has explicit tests for cancellation, disposal, errors, and partial results where applicable.

### 3. Documentation and usability

- [ ] README follows the Workshop reader journey and links to the canonical documentation map.
- [ ] First-use examples match current public API signatures and clearly distinguish fixture-based examples from live-provider execution.
- [ ] Provider guides list required settings, authentication headers, known limitations, and official API references.
- [ ] Security documentation explains secret handling, untrusted model output, outbound endpoint policy, and host-owned authorization.
- [ ] Roadmap and changelog distinguish implemented behavior from planned work.
- [ ] A new user can diagnose common configuration and request failures from documentation and error behavior.
- [ ] Claims about performance, token savings, accuracy, hallucinations, drift, and cost are backed by reproducible evidence or explicitly described as hypotheses.

### 4. Version and release control

- [ ] Confirm package ID, version, license, repository URL, package README, and dependency list.
- [ ] Confirm no unsupported compatibility promise is implied. The current project targets .NET 8.
- [ ] Confirm the workflow's NuGet publish condition still ends in `&& false`.
- [ ] Do not publish until the repository owner explicitly approves the release and the gate is intentionally changed for that approved release.
- [ ] After an approved release, restore the disabled gate to `&& false`.

## Known limitations to disclose

The following are known boundaries of the current foundation and should be reassessed as implementation evolves:

- Built-in provider adapters implement an initial text-focused subset of their APIs, not every provider feature.
- Streaming, rich multimodal payloads, provider-specific tool semantics, orchestration, retries, and global budgets are not promised by the current implementation.
- Direct HTTP does not bypass provider authentication, billing, quotas, rate limits, or access policies.
- A syntactically valid or successful response is not proof of semantic correctness or permission to act.
- No measured performance or token-efficiency claim should be made until benchmark and end-to-end experiments have been run and recorded.

## How to report a candidate

For each candidate, record:

1. Commit SHA and branch.
2. Build/test/coverage result and test count.
3. Package artifact name and package metadata inspection.
4. Known failures, limitations, and deferred gates.
5. Whether the candidate is merely packed, approved for release, or actually published.

**Packed is not published. CI passing is not release approval.** Keep those states explicit.

---

The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.

*Because state shouldn't be a mess.*

*And because static boundaries are invitations to cause trouble.*
