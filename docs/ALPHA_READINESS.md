<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Alpha Readiness — HeadlessAi

> **Purpose:** prepare a first release that is genuinely useful, understandable, and testable. “Alpha” describes maturity and the need for real-world feedback; it is not a reason to artificially withhold coherent functionality.



## Phone-first release review — 2026-10-22 UTC

Start with the [Release Review Brief](RELEASE_REVIEW_BRIEF.md) for the concise current contract, package boundaries, top remaining risks, and a guided reading order. It is a review aid, not a substitute for the checks below and not authorization to publish.

Latest candidate checkpoint: [a6e910b5ea6df2e5c85357fc92c176633b2da34e](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/commit/a6e910b5ea6df2e5c85357fc92c176633b2da34e). CI evidence: [workflow run #39725666027](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/39725666027). Current readiness estimate: **65%** (qualitative working estimate, not a test metric). Recheck CI on the exact final candidate after all changes.

## Latest verified CI baseline — 2026-10-22 UTC

**Latest green `development` commit:** `708c8ed34f318e32362165eb8d1fb3fa01a865e1`. This supersedes the older candidate recorded below in prior audit history.

- [x] GitHub Actions run [#39724307272](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/39724307272) passed for this exact commit.
- [x] The workflow verifies restore, Release build, automated tests/coverage, package creation, packed-artifact audit, and artifact upload.
- [x] The packed-artifact audit checks the assembly, XML docs, README, license metadata, package ID/version, internal documentation, gallery, and SVG assets.
- [x] Publication remains hard-disabled by `if: ${{ false }}`; manual dispatch cannot publish the package.
- [x] GitHub Actions references are pinned to immutable commit SHAs.
- [ ] Human API/contract/security/package review remains required; a green CI run is not release approval.

**Assessment:** this is the latest verified green baseline, not yet a publication-approved release candidate. Any subsequent commit—including this readiness-document update—must pass CI before its new head is treated as verified. Keep HeadlessAi independently releasable; do not hold its core alpha for streaming, multimodal support, orchestration, or the optional ProtocolAi/GrammarAi bridge.

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
- [x] Confirmed the current workflow uses `if: ${{ false }}`; the publication job is deliberately disabled regardless of manual-dispatch inputs.
- [ ] Do not publish until the repository owner explicitly approves the release. Re-enabling publication must be a separate, deliberate change after approval; do not weaken the gate as part of ordinary CI or documentation work.

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

---

<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://github.com/TrentBest.png?size=200" alt="The Singularity Workshop on GitHub" width="200">
  </a>
</p>

<p align="center">
  <a href="https://github.com/TrentBest">GitHub</a> ·
  <a href="https://coderlegion.com/">Coder Legion</a> ·
  <a href="https://www.patreon.com/c/TheSingularityWorkshop">Patreon</a> ·
  <a href="https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J">PayPal</a>
  <br>
  <a href="https://github.com/TrentBest/FSM_API">FSM_API</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization">FSM_Serialization</a> ·
  <a href="https://github.com/TrentBest/WebPage">WebPage</a>
</p>

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong><br><em>And because static boundaries are invitations to cause trouble.</em></p>
