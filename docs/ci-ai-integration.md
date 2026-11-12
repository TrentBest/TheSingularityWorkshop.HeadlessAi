<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# CiAi integration boundary

CiAi is the application-facing interface to CI facts and permitted operations. HeadlessAi is the transport seam to a configured LLM endpoint. These are complementary responsibilities, not competing abstractions.

## Request and response boundary

1. CiAi obtains run facts from the CI provider and validates the repository, run identity, commit, status, conclusion, and evidence URL.
2. The host prepares a bounded request using application-owned meanings. ProtocolAi can encode recognized terms as integer identities.
3. HeadlessAi sends the request through the selected endpoint adapter and returns normalized response content.
4. CiAi validates the response against the known vocabulary and the current host permission policy.
5. Only validated host code can perform an operation. A model response is never authorization.

HeadlessAi intentionally does not interpret protocol IDs, decide whether a CI run succeeded, or execute a command. It also does not eliminate the configured endpoint or HTTP transport; it avoids requiring one provider SDK in every consumer.

## Resumable repair evidence

For a bounded repair turn, HeadlessAi can preserve a host-accepted CiAi report as opaque UTF-8 evidence bound to the task, candidate commit, workflow run, and attempt. HeadlessAi checks exact-byte digest and identity consistency; it does not authenticate the CI provider, validate the verdict, or reinterpret the report. Foundry must authenticate the incoming artifact, verify persisted dispatch/run/candidate lineage, and accept it before constructing the context.

The model's response remains untrusted. Foundry validates the response against task and repository policy, bounds subsequent work, requests exact-SHA CI evidence, and hands the result to human review. HeadlessAi executes exactly one provider invocation per executor call; it does not own retries, durable idempotency, workflow state, repository mutations, or approval.

See [Agent turns and resumability](agent-turns-and-resumability.md) for the request, accepted-report, comparison, and binary persistence contracts.

## Package and dependency ordering for CiAi

HeadlessAi is a **NuGet library package**, not a MicroBundle derivative. Its current package identity is `TheSingularityWorkshop.HeadlessAi`, targeting `net8.0`. The next candidate is `0.1.0-alpha.2`; its required package dependency is `TheSingularityWorkshop.FSM_Serialization` at the exact published version `1.0.0`.

The project deliberately has two dependency modes:

- **Development verification:** the ordinary `build.yml` checks out FSM_Serialization `development` source and uses a `ProjectReference`. It builds/tests/collects coverage; it does not pack or publish.
- **Release verification:** `release-verification.yml` sets `UsePublishedDependencies=true` and supplies `FsmSerializationPackageVersion`. The project then uses an exact-version `PackageReference` to the already-published FSM_Serialization package.

The intended CiAi release sequence is:

1. Build and test FSM_Serialization; verify and publish its approved package through the central release authority.
2. Build HeadlessAi against the released FSM_Serialization version—not its moving development branch.
3. Run HeadlessAi's non-publishing release verification, which builds/tests against that exact package, packs one candidate, audits the archive/nuspec/docs, and builds an isolated clean consumer.
4. Preserve the verified package artifact and its exact source SHA, dependency versions, CI run/attempt, test result, and archive audit evidence.
5. Only after the owner authorizes this exact HeadlessAi package/version may the central release authority publish it.
6. Build downstream NuGet dependents against the released HeadlessAi package version. Do not substitute a source ProjectReference or a floating package range for a released dependency in a release build.

The repository does not implement a competing publication controller. CiAi should own cross-repository ordering, release authorization evidence, and dependent rebuild scheduling. The release-verification workflow exposes both manual `workflow_dispatch` and reusable `workflow_call` entry points. Both require an exact 40-character candidate SHA and exact published dependency version. The called workflow checks out that immutable HeadlessAi SHA, verifies and uploads a package artifact, and has no package-push step or write permission. CiAi can therefore invoke verification as one stage of its release graph after this workflow is integrated into the canonical branch; publication remains a separate central, owner-authorized operation.

The current dependency is explicit in `TheSingularityWorkshop.HeadlessAi.csproj`: development uses the sibling source project, while release packaging uses an exact package version passed by the orchestrator. The active candidate and remaining gates are recorded in [Next Release Candidate](RELEASE_CANDIDATE.md).

## Offline transport proof

The test suite includes fake HTTP endpoints that verify requests cross the HeadlessAi boundary and that out-of-vocabulary responses remain untrusted data rather than accepted operations. Tests require no credentials or external endpoint.

## First CiAi milestone

CiAi's validated CI evidence and safe feedback orchestration remain separate from HeadlessAi's provider invocation. See [CiAi](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/tree/development) for the central build-system implementation and release-graph status.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
