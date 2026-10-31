<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Continuation Brief — HeadlessAi

## Purpose and architectural boundary

HeadlessAi is a provider-neutral .NET boundary for direct HTTP model invocation and bounded, versioned agent-turn exchange. It preserves request identity, repository/commit provenance, CI evidence references, provider output, normalized proposals, and cross-provider comparisons.

HeadlessAi is **not** a workflow engine or autonomous agent loop. It does not own conversation storage, scheduling, retry loops, durable idempotency, provider arbitration, permissions, repository mutation, tool execution, or consequential actions.

- **Foundry** owns workflow lifecycle, durable duplicate suppression, bounded iteration, orchestration, authorization, and action handoff.
- **CiAi** supplies CI facts and evidence; HeadlessAi preserves evidence references as untrusted, provenance-bearing input.
- **ProtocolAi** and **GrammarAi** remain optional integrations. HeadlessAi must work without either.
- **FSM_Serialization** supplies the stable binary stream boundary used by the agent-turn codec. Do not introduce a competing stream abstraction.

## Repository invariants

- Keep provider endpoint schemas explicit. A provider adapter speaks its documented protocol; do not pretend URL substitution alone changes protocols.
- Reuse a host-injected `HttpClient`; HeadlessAi must not create independent connection pools for logical callers.
- Keep credentials host-managed and resolve rotating authorization headers at request time. Never persist credentials in prompts, evidence, metadata, or diagnostics.
- Treat raw provider output, normalized proposals, provider metadata, assessments, and CI evidence summaries as untrusted data.
- A model proposal is not permission to execute. The host independently validates every consequential action.
- Preserve cancellation and timeout boundaries. Do not add automatic retries; retry policy belongs to the host.
- Do not claim token savings, accuracy, hallucination reduction, or performance improvement without reproducible measurements.
- Keep NuGet publication disabled/skipped unless the user explicitly authorizes a release.

## Current implementation

### Endpoint API

- Reusable `HeadlessAiProfile` and `HeadlessAiAgentTemplate` configuration.
- Direct HTTP adapters for OpenAI Responses, Gemini generateContent, and Anthropic Messages.
- Raw-text, delegate-based, and JSON adapter extension points.
- Host-provided request headers, shared transport, cancellation, timeout, bounded response handling, normalized provider failures, and usage metadata.

### Versioned agent turns

- `HeadlessAiAgentTurnEnvelope` carries task/conversation identity, task version, isolated branch and commit provenance, expiry, idempotency key, bounded evidence/context, and resource limits.
- `HeadlessAiAgentTurnRequest` validates and snapshots canonical request JSON once. Providers share the same immutable request and SHA-256 fingerprint.
- `HeadlessAiAgentTurnExecutor` performs exactly one provider invocation and returns a provenance-bound result. It does not retry or deduplicate.
- `HeadlessAiAgentTurnComparison` validates shared provenance and explicit provider attribution. It does not infer consensus or select a winner.
- `HeadlessAiAgentTurnComparisonSnapshot` captures a bounded immutable canonical comparison payload and sorts provider-metadata keys so equivalent dictionaries produce identical serialized payloads and fingerprints.
- `HeadlessAiAgentTurnBinaryCodec` frames request and comparison snapshots over `TheSingularityWorkshop.FSM_Serialization` 1.0.0 and `IBinaryStream`.

The binary frame uses a 43-byte header (`HAIB` magic, kind, version, payload length, SHA-256 digest), with a 1,024,000-byte request payload limit and a 40,000,000-byte comparison payload limit. The digest detects accidental corruption; it is not a signature or authentication mechanism. The host owns stream lifetime, storage, encryption, access control, and retention.

## Current PR and verification posture

- Repository: `TrentBest/TheSingularityWorkshop.HeadlessAi`.
- Active work: PR [#11 — Add versioned resumable agent-turn contracts](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/pull/11).
- PR #11 is open and draft, from `feature/resumable-agent-turn-contracts` into `development`.
- Verified head before this documentation/test update: `c87386f0deb5f2f9f8a34dadc6811f2d59727c9f`.
- [Run #487](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/41174034167) completed successfully on that exact head. The `verify` job passed restore, build, tests with coverage, Codecov upload, pack, package audit, clean-consumer verification, and artifact upload.
- The `publish_nuget` job was skipped. No package was published.
- The previous Codecov comment recorded 94.36090% patch coverage and 45 changed lines missing coverage; that report predates the latest changes and is not represented as current coverage.
- A targeted binary-codec review found that comparison snapshots had round-trip coverage but lacked malformed-frame regression coverage equivalent to the request-frame tests. The current update adds comparison-specific tests for unsupported versions, payload tampering, wrong frame kind, truncated headers/payloads, invalid UTF-8 with a matching digest, oversized declared lengths, noncanonical JSON with a matching digest, and invalid comparison provenance with a correctly framed/checksummed payload.
- The implementation itself already shares the bounded frame reader/writer and revalidates comparison snapshots on unpack; this was a coverage gap, not a demonstrated codec defect.

No merge, release, NuGet publication, or writes to `master` / `development` are included.

## Next logical work

1. Continue the snapshot and binary-codec boundary audit, focusing on meaningful malformed-payload and exact-boundary behavior rather than defensive branches that callers cannot reach.
2. Review the agent-turn guide and public XML/API docs against the actual implementation; generated XML documentation is enabled and warnings are treated as errors.
3. Check Foundry's consumption boundary: Foundry should own durable replay/idempotency, conversation history, authorization, bounded iteration, and action handoff.
4. Keep provider adapters and security behavior aligned with their documented endpoint contracts; use deterministic fixtures for regression coverage.
5. Expand benchmarks only where a stable, meaningful operation exists; publish no performance claim without reproducible baselines.
6. Keep the PR open and draft until the implementation, documentation, coverage, and integration review are complete.

## Documentation and branch discipline

- Follow the current Workshop documentation standard: animated moniker at the top and the canonical PNG footer below.
- Keep public documentation operational and user-facing; avoid stale branch/run notes outside this continuation brief.
- The intended long-lived branches are `master` (released state) and `development` (active integration). Feature work stays isolated in its PR until explicitly approved.
- Do not merge, publish, delete branches, or change protected branch refs without the user's explicit direction.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
