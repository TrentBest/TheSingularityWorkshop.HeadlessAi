<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# IHeadlessAiAdapter

## Purpose

Provider-protocol seam for constructing requests and extracting normalized response content.

## Declaration and ownership

- **Declaration:** public interface.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/IHeadlessAiAdapter.cs](src/TheSingularityWorkshop.HeadlessAi/IHeadlessAiAdapter.cs).
- **Related types:** HeadlessAiAgent; HeadlessAiAgentTemplate; RawTextHeadlessAiAdapter; JsonHeadlessAiAdapter; OpenAiResponsesAdapter; GeminiGenerateContentAdapter; AnthropicMessagesAdapter.
- **Responsibilities:** Separates endpoint-specific wire formats from generic agent invocation.
- **Non-goals:** It does not make provider protocols equivalent or supply authorization policy.

## Contract notes

The host owns task planning, credentials, permissions, response validation, budgets, and consequential actions. A successful HTTP exchange is not proof that a response is safe or correct.

## Verification

Relevant evidence: [tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs](tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs). This page is source-authored and checked against the current declaration; it is not generated from reflection or compiler AST.

## Related documentation

- [Problem domain](../problem-domain.md)
- [Architecture boundaries](../architecture-boundaries.md)
- [Usage guide](../usage.md)
- [Companion UML and use cases](../UML/IHeadlessAiAdapterUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
