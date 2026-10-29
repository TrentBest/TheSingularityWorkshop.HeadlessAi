<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# HeadlessAiInput

## Purpose

One host-supplied request content value with optional metadata.

## Declaration and ownership

- **Declaration:** public record.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/HeadlessAiInput.cs](src/TheSingularityWorkshop.HeadlessAi/HeadlessAiInput.cs).
- **Related types:** HeadlessAiAgent; IHeadlessAiAdapter.
- **Responsibilities:** Carries content and optional metadata into the selected adapter.
- **Non-goals:** It does not authorize endpoint access to host data or tools.

## Contract notes

The host owns task planning, credentials, permissions, response validation, budgets, retries where appropriate, and consequential actions. A successful HTTP exchange is not proof that a response is safe or correct.

## Verification

Relevant evidence: [tests/TheSingularityWorkshop.HeadlessAi.Tests/HeadlessAiTests.cs](tests/TheSingularityWorkshop.HeadlessAi.Tests/HeadlessAiTests.cs). This page is source-authored and checked against the current declaration; it is not generated from reflection or compiler AST.

## Related documentation

- [Problem domain](../problem-domain.md)
- [Architecture boundaries](../architecture-boundaries.md)
- [Usage guide](../usage.md)
- [Companion UML and use cases](../UML/HeadlessAiInputUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
