<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# OpenAiResponsesAdapter

## Purpose

Adapter for the OpenAI Responses HTTP request/response protocol.

## Declaration and ownership

- **Declaration:** public sealed adapter class.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/OpenAiResponsesAdapter.cs](../../src/TheSingularityWorkshop.HeadlessAi/OpenAiResponsesAdapter.cs).
- **Related types:** IHeadlessAiAdapter; JsonHeadlessAiAdapter; HeadlessAiProfile; HeadlessAiInput; HeadlessAiOutput.
- **Responsibilities:** Maps the normalized input and profile settings to the Responses endpoint contract and extracts its response content.
- **Non-goals:** It does not call the public chat website, select a model on behalf of the host, or own API credentials.

## Verification

Relevant evidence: [tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs](../../tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs). The page is source-authored and source-checked, not generated.

## Related documentation

- [Provider adapters](../provider-adapters.md)
- [Problem domain](../problem-domain.md)
- [Companion UML and use cases](../UML/OpenAiResponsesAdapterUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
