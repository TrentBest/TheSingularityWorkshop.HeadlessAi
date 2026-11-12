<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# HeadlessAi — Next Release Candidate

**Review date:** 2026-11-12 UTC  
**Purpose:** one evidence-backed status page for the next HeadlessAi NuGet package candidate. This document is a release-preparation record, not authorization to merge or publish.

## Candidate identity

| Field | Candidate |
|---|---|
| Package | `TheSingularityWorkshop.HeadlessAi` |
| Previous published version | `0.1.0-alpha.1` |
| Next candidate version | `0.1.0-alpha.2` |
| Target framework | `net8.0` |
| License | MIT |
| Required package dependency | `TheSingularityWorkshop.FSM_Serialization 1.0.0` |
| Candidate branch | `feature/resumable-agent-turn-integration` |
| Candidate commit at review start | `5ca7b42af7bb90453f5ef4334c594a291771b36a` |
| Current candidate head | `0cd0e888f8ffa1bc98f3bfd10013b325bf4d800f` |
| Integration PR | [#13 — Integrate v1 resumable agent-turn contracts](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/pull/13) |

NuGet currently lists `0.1.0-alpha.1` as the published HeadlessAi version. The next package must not reuse that immutable version. The project version has therefore been advanced to `0.1.0-alpha.2` for this candidate. See the [NuGet package listing](https://www.nuget.org/packages/TheSingularityWorkshop.HeadlessAi).

## Domain delivered by this package

HeadlessAi is a provider-neutral .NET library for bounded HTTP invocation against configured AI-capable endpoints, with explicit protocol adapters and versioned resumable-turn exchange contracts.

- **Invocation:** immutable endpoint profiles, templates, host-injected `HttpClient`, request-time header providers, cancellation, timeout, bounded response buffering, bounded error excerpts, and explicit failure metadata.
- **Provider protocols:** raw text, delegate, JSON, OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters. These are tested protocol slices, not a promise of universal provider feature parity.
- **Agent turns:** bounded v1 envelopes, canonical immutable request snapshots, request fingerprints, one-shot provider execution, provider-attributed results, comparisons/assessments, and versioned binary frames.
- **CI evidence:** accepts host-supplied CiAi report bytes and lineage as opaque, untrusted evidence. It verifies byte/digest/identity consistency, not authenticity or the CI verdict. Foundry must authenticate and accept the report before constructing the context.
- **Persistence:** binary codec uses the published FSM_Serialization abstraction. The host owns storage, encryption, retention, access control, replay policy, idempotency, and concurrency.
- **Authority:** Foundry owns task/workflow lifecycle, authorization, durable replay/idempotency, repository operations, and human handoff. CiAi owns CI facts and accepted evidence. HeadlessAi does not retry, execute tools, mutate repositories, or publish packages.

## Verification evidence

- [x] Development Build, Test, and Coverage passed for exact PR head `5ca7b42af7bb90453f5ef4334c594a291771b36a`: [run #43160452720](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/43160452720).
- [x] Build succeeded; 248 tests passed, 0 failed, 0 skipped.
- [x] Coverage preparation and Codecov upload succeeded.
- [x] Ordinary development workflow uses FSM_Serialization `development` source and does not pack or publish.
- [x] Release verification workflow is manual-only, read-only, and contains no NuGet push, release creation, deployment, merge, or write permission.
- [ ] Run the manual non-publishing release verification on the exact `0.1.0-alpha.2` candidate.
- [ ] Confirm the published FSM_Serialization `1.0.0` dependency is restored from NuGet in that run.
- [ ] Inspect the resulting `.nupkg` artifact and nuspec, including assembly, XML docs, README, license, Markdown/SVG assets, exact dependency allowlist, and package ID/version.
- [ ] Confirm the isolated clean-consumer project installs the produced package and builds successfully.
- [ ] Review the actual public API, package contents, and documented limitations before release approval.

The manual verification workflow has not yet been run. The connected GitHub actions available in this session do not expose a workflow-dispatch operation, so the workflow must be started from the repository's Actions page. Its workflow is intentionally non-publishing.

## Repository and ecosystem gates

- [ ] Reconcile the long-lived feature branch and PR #13 into the permanent `development` branch through the owner's review/merge process.
- [ ] Restore the exact two-branch Sith policy: `master` and `development`. Current inventory has seven persistent branches; no surplus branch is presently proven safe to delete.
- [ ] Keep dependency ordering explicit: FSM_Serialization `1.0.0` is the already-published dependency used for this HeadlessAi candidate. A later dependency change requires its own published package version and a clean rebuild.
- [ ] Register HeadlessAi in CiAi's repository dependency/build graph so the release pipeline builds this package after its dependencies, records exact candidate evidence, and builds dependents only against the released package.
- [ ] Preserve the split between NuGet library packages and MicroBundle derivatives: HeadlessAi is a NuGet library; it is not a MicroBundle derivative.

The repository's branch cleanup is owner-controlled. The assistant does not delete branches. No merge, NuGet publication, release, deployment, or branch deletion has occurred.

## Release state vocabulary

- **Source verified:** exact commit has passed the repository's source build/test workflow.
- **Package verified:** the exact package has passed archive inspection and clean-consumer installation/build.
- **Release approved:** the owner has explicitly authorized the specific package ID and version.
- **Published:** NuGet confirms that exact package/version is available.

A source-verified commit is not a package-verified artifact. A package-verified artifact is not release approval. A published package is the only state that downstream package consumers should treat as a released dependency.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
