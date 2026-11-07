<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Continuation Brief — HeadlessAi

## Purpose

HeadlessAi is a standalone .NET HTTP-agent toolkit. It configures endpoint-specific HTTP requests and responses and lets hosts create lightweight agent instances from reusable profiles/templates. It is not itself a model, workflow engine, permission system, memory store, or execution loop.

## Architecture invariants

- Keep endpoint-specific schemas explicit; do not force all providers into one JSON body.
- Share an injected HttpClient; agent instances must not own independent connection pools.
- Keep ProtocolAi and GrammarAi optional. HeadlessAi must work without them and must not depend on TheForge, FSM_COS, or FSM_API.
- The host owns roles, memory, scheduling, permissions, budgets, validation, tool mediation, and consequential actions.
- Treat model output as untrusted data, never as permission to execute.
- Use request-time header providers for rotating secrets; avoid secret-bearing diagnostics.
- Bound response bodies, timeouts, and cancellation behavior.
- Never claim token savings, accuracy, hallucination reduction, or performance improvement without reproducible measurements.
- Never publish to NuGet without explicit user approval. CI publication must remain disabled by default.

## Current repository structure

- src/TheSingularityWorkshop.HeadlessAi/: library contracts and provider adapters.
- tests/TheSingularityWorkshop.HeadlessAi.Tests/: deterministic fixture-based tests.
- benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks/: local BenchmarkDotNet baselines.
- docs/: problem domain, theory, architecture, usage, provider adapter, security, performance, use cases, measurement, and roadmap documentation.
- `.github/workflows/build.yml`: development-source checkout, restore, Release build, tests, and coverage/Codecov. Development CI does not pack or publish NuGet artifacts.

## Versioned agent-turn boundary

- `HeadlessAiAgentTurnEnvelope` carries bounded task/conversation identity, isolated branch and commit provenance, expiry, idempotency identity, evidence references, and resource limits.
- `HeadlessAiAgentTurnRequest` validates and snapshots canonical request JSON once; providers share the immutable request and SHA-256 fingerprint.
- `HeadlessAiAgentTurnExecutor` performs one provider invocation. It does not retry, deduplicate, authorize actions, or own durable conversation state.
- `HeadlessAiAgentTurnComparison` preserves independent provider results and explicitly attributed assessments; it does not infer consensus or choose a winner.
- `HeadlessAiAgentTurnComparisonSnapshot` captures a bounded canonical comparison payload.
- `HeadlessAiAgentTurnBinaryCodec` frames request and comparison snapshots over the `FSM_Serialization` project reference and `IBinaryStream`.

The binary frame uses a 43-byte header (`HAIB` magic, kind, version, payload length, SHA-256 digest), with a 2,000,000-byte request payload limit and a 40,000,000-byte comparison payload limit. The digest detects corruption; it is not a signature or authentication mechanism. The host owns stream lifetime, storage, encryption, access control, and retention.

**Responsibility split:** Foundry owns lifecycle, durable replay/idempotency, orchestration, authorization, budgets, and action handoff. CiAi supplies CI facts; HeadlessAi preserves evidence references as untrusted, provenance-bearing input. FSM_Serialization supplies the shared binary stream boundary. HeadlessAi must not introduce a competing stream abstraction or depend on Foundry/CiAi.

## Current implementation slice

- Endpoint profiles and reusable agent templates.
- Raw text, delegate-based, and JSON adapter extension points.
- Direct HTTP adapters for OpenAI Responses, Gemini generateContent, and Anthropic Messages.
- Request-time header resolution, cancellation, timeouts, and bounded response bodies.
- Usage metadata extraction for supported response fields.
- Fixture-based provider request/response tests.
- BenchmarkDotNet project and CI coverage/pack workflow.

The built-in adapters currently focus on basic text requests. Streaming, rich multimodal inputs, full provider tool semantics, broad error-envelope normalization, explicit redirect policy, retry policy, and host-level rate/cost budgets remain future work or host responsibilities.

## Current branch and review posture

- Active integration: [PR #13](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/pull/13), from `feature/resumable-agent-turn-integration` into `development`.
- PR #11 remains open and untouched; it is not the integration source because its history diverged substantially from current `development`.
- The integration branch was created from current `development` and selectively ports the v1 implementation, tests, and guide while retaining current build, quickstart, consumer-verification, and AI-onboarding work.
- Previous verified head: `5cc008c533ab704f9bbf08af47068b35f0678dc6` (Run #567). That run predates the corrected development dependency lifecycle and is not evidence for the current workflow.
- Current development CI checks out `TrentBest/TheSingularityWorkshop.FSM_Serialization` at `development` and builds through a project reference. It does not pack, audit, or publish NuGet packages.
- Release packaging is a separate future gate: it must use an exact already-published FSM_Serialization dependency version with `UsePublishedDependencies=true`, after all release gates pass. Publication still requires the user's explicit authorization.
- No merge, release, deployment, or write to `master` or `development` is included.

## Next logical work

1. Review the v1 contracts and binary codec for domain completeness, edge cases, and public API clarity; retain strict rejection of unsupported schema/frame versions.
2. Review generated XML docs and the agent-turn guide against the implemented public API; keep examples compiling and claims bounded.
3. Confirm Foundry's consumption boundary: Foundry owns durable replay/idempotency, conversation history, authorization, bounded iteration, and action handoff.
4. Keep the optional bounded artifact HTTP adapter separate until FSM_REST supplies the shared bounded streaming/binary contract tracked in FSM_REST issue #13 and HeadlessAi issue #12.
5. Expand benchmarks only for stable, meaningful operations; make no performance claim without reproducible baselines.
6. Keep PR #13 draft until integration review and any follow-up changes are complete; do not merge or publish without explicit user authorization.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
