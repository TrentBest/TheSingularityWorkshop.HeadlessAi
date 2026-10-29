<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# RawTextHeadlessAiAdapter

## Purpose

Raw-text HTTP adapter for endpoints whose request and response body are plain text.

## Declaration and ownership

- **Declaration:** public sealed adapter class.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/RawTextHeadlessAiAdapter.cs](../../src/TheSingularityWorkshop.HeadlessAi/RawTextHeadlessAiAdapter.cs).
- **Related types:** IHeadlessAiAdapter; HeadlessAiProfile; HeadlessAiInput; HeadlessAiOutput.
- **Responsibilities:** Creates a request from the profile and input and extracts bounded response text.
- **Non-goals:** It does not define provider-specific JSON semantics or decide whether returned text is safe.

## Verification

Relevant tests: [tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs](../../tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs). This page is source-authored and source-checked, not generated.

## Related documentation

- [Provider adapters](../provider-adapters.md)
- [Problem domain](../problem-domain.md)
- [Companion UML and use cases](../UML/RawTextHeadlessAiAdapterUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
