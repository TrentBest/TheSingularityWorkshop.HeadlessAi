<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# HeadlessAiOutput

## Purpose

Normalized response content with HTTP status, media type, and optional metadata.

## Declaration and ownership

- **Declaration:** public record.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/HeadlessAiOutput.cs](../../src/TheSingularityWorkshop.HeadlessAi/HeadlessAiOutput.cs).
- **Related types:** HeadlessAiAgent; IHeadlessAiAdapter.
- **Responsibilities:** Carries adapter-extracted output to the host for evaluation.
- **Non-goals:** It does not certify truth, correctness, user intent, or permission to execute a command.

## Contract notes

The host owns task planning, credentials, permissions, response validation, budgets, and consequential actions. A successful HTTP exchange is not proof that a response is safe or correct.

## Verification

Relevant evidence: [tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs](../../tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs). This page is source-authored and checked against the current declaration; it is not generated from reflection or compiler AST.

## Related documentation

- [Problem domain](../problem-domain.md)
- [Architecture boundaries](../architecture-boundaries.md)
- [Usage guide](../usage.md)
- [Companion UML and use cases](../UML/HeadlessAiOutputUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
