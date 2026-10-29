<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# JsonHeadlessAiAdapter

## Purpose

Generic JSON request/response adapter for an explicitly configured JSON endpoint contract.

## Declaration and ownership

- **Declaration:** public sealed adapter class.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/JsonHeadlessAiAdapter.cs](../../src/TheSingularityWorkshop.HeadlessAi/JsonHeadlessAiAdapter.cs).
- **Related types:** IHeadlessAiAdapter; HeadlessAiProfile; HeadlessAiInput; HeadlessAiOutput.
- **Responsibilities:** Builds JSON requests from profile settings and extracts normalized content through defined JSON handling.
- **Non-goals:** It is not a universal schema that makes all provider JSON protocols interchangeable.

## Verification

Relevant tests: [tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs](../../tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs). This page is source-authored and source-checked, not generated.

## Related documentation

- [Provider adapters](../provider-adapters.md)
- [Problem domain](../problem-domain.md)
- [Companion UML and use cases](../UML/JsonHeadlessAiAdapterUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
