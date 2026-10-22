# HeadlessAi Domain Completeness Plan

**Purpose:** define the smallest coherent domain HeadlessAi owns, verify it against implementation and tests, and make every public promise traceable to evidence. This is the working plan for alpha release hardening—not a claim that every item is already complete.

## 1. Domain definition

HeadlessAi is a provider-neutral .NET library for making bounded, explicit HTTP invocations against configured AI-capable endpoints. It maps normalized input through a selected endpoint protocol and returns extracted content plus available metadata. It does not decide why to call, what the answer means, or what the application should do next.

### Core vocabulary

| Concept | Responsibility | Boundary |
|---|---|---|
| Profile | Immutable endpoint URI, method, headers, timeout, response bound, and non-secret protocol settings | Configuration, not a secret vault or trust decision |
| Adapter | Maps normalized input to a protocol-specific request and parses a protocol-specific response | Explicit protocol slice, not universal provider equivalence |
| Header provider | Supplies request-time headers for rotation and host-managed credentials | Host owns secret storage and authorization |
| Template | Reusable profile + adapter + optional header provider | Configuration, not an autonomous agent loop |
| Agent | Sends an invocation through host-injected HTTP transport | Lightweight caller, not a thread, model, memory, or permission grant |
| Input / result | Normalized prompt/input, extracted content, and available metadata | Text-focused current contract; provider-specific features remain explicit |
| Host | Owns task intent, policy, memory, concurrency, budgets, validation, and actions | Authority stays outside HeadlessAi |
| Endpoint | Performs the configured remote operation | Authentication, availability, billing, quotas, and semantics remain endpoint-specific |

### In scope today

- Reusable endpoint profiles and templates, and lightweight agent invocation.
- Host-injected `HttpClient`; explicit HTTP method and endpoint ownership.
- Raw-text, delegate, JSON, OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters.
- Request-time headers, cancellation, configured timeouts, bounded successful response bodies, and bounded non-success excerpts.
- Normalized response content and usage metadata where the adapter can extract it.
- Custom adapter extension points, deterministic fixture tests, package documentation, and offline benchmarks.

### Explicitly out of scope for the core alpha

- Browser/GUI automation or access to a signed-in chat website.
- Agent planning, autonomous loops, tool execution, application permissions, memory, or workflow/state-machine orchestration.
- Required dependencies on ProtocolAi, GrammarAi, FSM_API, FSM_COS, TheForge, or provider SDKs.
- Claims of universal provider compatibility; full provider schemas; streaming; rich multimodal content; provider-specific tool calling; retries; global rate/cost budgets; semantic correctness.
- Silent redirect policy or complete SSRF prevention. The host controls `HttpClient` and must enforce destination policy.
- A fluent builder as a shipped API: current public configuration is constructor-based.

## 2. Definition of done by domain outcome

- [ ] **Connect:** document and test valid profile construction, URI/HTTPS policy, local HTTP opt-in, timeout, response bounds, method, and required adapter settings.
- [ ] **Invoke:** compile the documented first-use example against the exact current public API; verify request method, URI, headers, and body with a fake handler.
- [ ] **Interpret:** define content and metadata guarantees; document missing text, blocked/no-text results, and provider-specific extraction limits.
- [ ] **Fail clearly:** verify cancellation versus timeout, network failures, non-success HTTP, oversized bodies, malformed/empty payloads, invalid settings, and bounded error excerpts.
- [ ] **Extend:** demonstrate a small custom adapter and document the stable extension contract and disposal/thread-safety expectations.
- [ ] **Secure:** document secret handling, redirects, outbound allowlists/SSRF, untrusted outputs, safe logging, and host-owned authorization; test security-relevant boundaries.
- [ ] **Operate:** define caller concurrency and reuse of `HttpClient`; document that retries, budgets, and idempotency are host policies.
- [ ] **Learn:** README first-minute example, usage guide, provider setup, failure contract, security model, gallery, and source-derived class/API documentation agree.
- [ ] **Package:** inspect the actual .nupkg contents and metadata, verify examples and relative links, version/license/repository metadata, and framework support policy.
- [ ] **Release safely:** test the exact candidate, record evidence and unresolved gates, retain immutable action pins, and keep publication disabled until explicit owner approval.

## 3. Provider coverage matrix

For each built-in adapter, track exact official API endpoint/version, required settings and headers, request fixture, supported response variants, usage metadata, malformed/missing/empty payload behavior, error envelope behavior, and limitations.

- [ ] OpenAI Responses API
- [ ] Gemini generateContent
- [ ] Anthropic Messages API
- [ ] Generic JSON adapter
- [ ] Delegate adapter
- [ ] Raw-text adapter

Fixture coverage proves only the tested protocol shapes. Before claiming current provider compatibility, compare fixtures and behavior against current official documentation and record the checked date.

## 4. Ordered plan of attack

### P0 — Align release truth
- [ ] Update candidate SHA/run evidence to the latest development head; never carry forward an older green run as evidence for a new candidate.
- [ ] Keep the publish job hard-disabled and confirm the packaged ID/version matches the project.
- [ ] Refresh the branch ledger after documentation changes; preserve master-only useful identity/docs and keep all branch decisions owner-controlled.

### P1 — Prove the public contract
- [ ] Inspect public source/API, constructor validation, exception behavior, XML docs, and nullable annotations.
- [ ] Add missing deterministic tests from the definition-of-done list, prioritizing cancellation/timeout distinction, non-success status, oversized bodies, header precedence, endpoint restrictions, and malformed adapter variants.
- [ ] Compile README and usage examples in a consumer smoke test; avoid live credentials in CI.

### P2 — Close domain documentation
- [ ] Reconcile this document, Problem Domain, Usage, Provider Adapters, Provider Failure Contract, Security, Architecture Boundaries, Roadmap, Gallery, README, and package metadata against actual source/tests.
- [ ] Add or verify per-public-class companion documentation and focused UML class cards/diagrams following the Workshop standard. Keep authored architecture art explicitly explanatory; do not label it source-generated or live evidence.
- [ ] Add provider-specific official-reference/check dates and a clear current-vs-proposed matrix for integrations and features.

### P3 — Verify artifact and readiness
- [ ] Inspect packed .nupkg contents and nuspec manually in addition to CI checks.
- [ ] Run Release restore/build/test/pack on the exact candidate and record test count, coverage evidence, artifact, and CI run.
- [ ] Review remaining unchecked gates and assign each one a disposition: complete with evidence, implement, or explicitly defer with rationale.
- [ ] Only then report a defensible readiness percentage and ask the owner for release approval. No merge, branch deletion, or publication is implied by this plan.

## 5. Evidence rule

Every checked item needs a source/test/CI/artifact reference or a short reproducible verification note. A documentation statement is not proof of implementation; a passing fixture is not proof of every provider variant; a packed artifact is not a published package; a green CI run is not release approval.

---

The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.
