<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# HeadlessAiHttpException

## Purpose

HTTP failure carrying profile identity, status code, and a bounded response excerpt.

## Declaration and ownership

- **Declaration:** public class.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/HeadlessAiHttpException.cs](../../src/TheSingularityWorkshop.HeadlessAi/HeadlessAiHttpException.cs).
- **Related types:** HeadlessAiAgent; HeadlessAiProfile.
- **Responsibilities:** Makes endpoint failures diagnosable while preserving context about the selected profile.
- **Non-goals:** It does not decide whether retrying is safe or whether a workflow should continue.

## Contract notes

The host owns task planning, credentials, permissions, response validation, budgets, and consequential actions. A successful HTTP exchange is not proof that a response is safe or correct.

## Verification

Relevant evidence: [tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterFailureTests.cs](../../tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterFailureTests.cs). This page is source-authored and checked against the current declaration; it is not generated from reflection or compiler AST.

## Related documentation

- [Problem domain](../problem-domain.md)
- [Architecture boundaries](../architecture-boundaries.md)
- [Usage guide](../usage.md)
- [Companion UML and use cases](../UML/HeadlessAiHttpExceptionUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
