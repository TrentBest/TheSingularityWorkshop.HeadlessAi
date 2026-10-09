# Alpha Readiness — HeadlessAi

> **Purpose:** make the first public alpha understandable, inspectable, and safe to evaluate. An alpha is an invitation to test a clearly bounded contract—not a claim of production maturity.

## What the alpha is

HeadlessAi is a .NET 8 library for invoking configured AI-capable HTTP endpoints through explicit protocol adapters. The initial scope is deliberately narrow:

- Reusable endpoint profiles and agent templates.
- A lightweight agent invocation surface using a host-injected `HttpClient`.
- Raw-text, delegate-based, JSON, OpenAI Responses, Gemini `generateContent`, and Anthropic Messages adapters.
- Request-time header providers, cancellation, timeouts, and bounded response bodies.
- Normalized content and provider usage metadata where the response supplies it.

Built-in provider adapters are text-focused and non-streaming. The package does not supply conversation memory, autonomous planning, tool execution, authorization, global rate/cost budgets, or a GUI. ProtocolAi and GrammarAi remain optional.

## Release gates

Use this checklist against the exact candidate commit. Do not infer readiness from an older successful run.

### 1. Build and package

- [ ] Restore and Release build succeed with warnings treated as errors.
- [ ] Unit tests pass without live provider calls or credentials.
- [ ] Coverage report is produced and uploaded successfully.
- [ ] Package can be packed from a clean checkout.
- [ ] Inspect the generated `.nupkg`: verify assembly, XML documentation, README, license metadata, package ID, version, target framework, and absence of secrets or unintended files.
- [ ] Record the candidate commit SHA and retain the package artifact.

### 2. Contract and failure-path tests

- [ ] Profile validation covers absolute URI, HTTPS-by-default, explicit local HTTP opt-in, positive response limit, and timeout rules.
- [ ] Transport tests cover caller cancellation, configured timeout, non-success HTTP status, oversized success body, and bounded error excerpts.
- [ ] Header tests cover request-time resolution and dynamic override behavior without leaking values.
- [ ] Adapter tests cover valid responses, missing/malformed required fields, empty text, provider error responses, and usage metadata when present.
- [ ] Tests use deterministic fixtures and fake HTTP handlers; CI does not depend on paid endpoints.

### 3. Documentation and usability

- [ ] README follows the Workshop reader journey and links to the canonical documentation map.
- [ ] The first-use example matches current public API signatures and clearly distinguishes source-checked examples from live-provider execution.
- [ ] Provider guides list required settings, authentication headers, known limitations, and official API references.
- [ ] Security documentation explains secret handling, untrusted model output, outbound endpoint policy, and host-owned authorization.
- [ ] Roadmap and changelog distinguish implemented behavior from planned work.
- [ ] Claims about performance, token savings, accuracy, hallucinations, drift, and cost are either backed by reproducible evidence or explicitly described as hypotheses.

### 4. Version and release control

- [ ] Confirm package ID, version, license, repository URL, package README, and dependency list.
- [ ] Confirm no unsupported compatibility promise is implied. The current project targets .NET 8.
- [ ] Confirm the workflow's NuGet publish condition still ends in `&& false`.
- [ ] Do not publish until the repository owner explicitly approves the release and the gate is intentionally changed for that approved release.
- [ ] After an approved release, restore the disabled gate to `&& false`.

## Known limitations to disclose

- Provider adapters implement an initial text-focused subset of their APIs, not every provider feature.
- Streaming, rich multimodal payloads, provider-specific tool semantics, orchestration, retries, and global budgets are not promised by this package.
- Direct HTTP does not bypass provider authentication, billing, quotas, rate limits, or access policies.
- A syntactically valid or successful response is not proof of semantic correctness or permission to act.
- The package has no measured performance or token-efficiency claim until benchmark and end-to-end experiments have been run and recorded.

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
