<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# GeminiGenerateContentAdapter

## Purpose

Adapter for the Gemini generateContent HTTP request/response protocol.

## Declaration and ownership

- **Declaration:** public sealed adapter class.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/GeminiGenerateContentAdapter.cs](../../src/TheSingularityWorkshop.HeadlessAi/GeminiGenerateContentAdapter.cs).
- **Related types:** IHeadlessAiAdapter; JsonHeadlessAiAdapter; HeadlessAiProfile; HeadlessAiInput; HeadlessAiOutput.
- **Responsibilities:** Maps profile settings and input into the endpoint's JSON contract and normalizes the response.
- **Non-goals:** It does not guarantee provider availability, model correctness, or host authorization.

## Verification

Relevant evidence: [tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs](../../tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs). The page is source-authored and source-checked, not generated.

## Related documentation

- [Provider adapters](../provider-adapters.md)
- [Problem domain](../problem-domain.md)
- [Companion UML and use cases](../UML/GeminiGenerateContentAdapterUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
