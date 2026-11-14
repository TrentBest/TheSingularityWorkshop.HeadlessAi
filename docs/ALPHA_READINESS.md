<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Alpha Readiness — HeadlessAi

The active, evidence-backed release status is maintained in the [Next Release Candidate](RELEASE_CANDIDATE.md). That page supersedes historical candidate SHAs and readiness estimates in this document.

## Release ambition

HeadlessAi should be a useful, independently consumable .NET library for bounded AI-capable HTTP invocation, explicit provider protocol adapters, and resumable provider-neutral agent-turn exchange contracts. Alpha status is not a reason to withhold coherent functionality, but it is also not permission to claim unimplemented provider features.

The release standard is **broad usefulness with honest boundaries**:

- Keep the core independently usable and provider-extensible.
- Make provider differences explicit rather than implying universal compatibility.
- Use deterministic fixtures for protocol, failure, cancellation, timeout, security, and persistence tests.
- Keep credentials, authorization, workflow lifecycle, storage, retries, idempotency, and consequential actions in the host/orchestrator.
- Make every readiness claim traceable to source, tests, package contents, or an exact CI run.

## Current domain

The current package candidate includes:

- Reusable endpoint profiles and templates with host-injected `HttpClient`.
- Raw-text, delegate, JSON, OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters.
- Request-time header providers, cancellation, configured timeouts, bounded successful responses, and bounded non-success excerpts.
- Versioned agent-turn envelopes, immutable canonical requests, request fingerprints, one-shot execution, provider comparisons and assessments, and binary persistence through FSM_Serialization.
- Host-supplied accepted CiAi report context whose exact bytes, digest, identity, and candidate lineage are consistency-checked but not authenticated by HeadlessAi.

## Explicit boundaries

HeadlessAi does not provide browser automation, autonomous planning, workflow scheduling, repository operations, authorization, durable replay/idempotency, automatic retries, conversation storage, provider-wide feature parity, streaming, rich multimodal payloads, or provider-specific tool execution. These are either host/orchestrator responsibilities or future work. A valid model response or CI report digest does not grant authority.

## Release gates

Use the [Next Release Candidate](RELEASE_CANDIDATE.md) as the active checklist. In particular:

1. Run the exact-head Development Build, Test, and Coverage workflow.
2. Run the manual non-publishing NuGet release-verification workflow against the published FSM_Serialization dependency version.
3. Inspect the generated package archive and nuspec, including package identity/version, dependency allowlist, assembly, XML API docs, README, license, Markdown/SVG documentation, and README link portability.
4. Install the exact generated package into an isolated clean consumer project and build it.
5. Reconcile PR #13, branch policy, package dependency ordering, and CiAi's future release graph.
6. Obtain explicit owner approval for the specific package ID/version before publication.

**Packed is not published. Source CI passing is not package verification. Package verification is not release approval.**

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
