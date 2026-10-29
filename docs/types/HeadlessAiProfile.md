<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# HeadlessAiProfile

## Purpose

Immutable endpoint configuration including URI, method, headers, settings, timeout, and maximum response size.

## Declaration and ownership

- **Declaration:** public class.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/HeadlessAiProfile.cs](src/TheSingularityWorkshop.HeadlessAi/HeadlessAiProfile.cs).
- **Related types:** HeadlessAiAgent; HeadlessAiAgentTemplate; IHeadlessAiAdapter.
- **Responsibilities:** Validates and snapshots configuration used by an adapter and agent.
- **Non-goals:** It is not a secret vault or proof that an endpoint is trusted.

## Contract notes

The host owns task planning, credentials, permissions, response validation, budgets, retries where appropriate, and consequential actions. A successful HTTP exchange is not proof that a response is safe or correct.

## Verification

Relevant evidence: [tests/TheSingularityWorkshop.HeadlessAi.Tests/HeadlessAiTests.cs](tests/TheSingularityWorkshop.HeadlessAi.Tests/HeadlessAiTests.cs). This page is source-authored and checked against the current declaration; it is not generated from reflection or compiler AST.

## Related documentation

- [Problem domain](../problem-domain.md)
- [Architecture boundaries](../architecture-boundaries.md)
- [Usage guide](../usage.md)
- [Companion UML and use cases](../UML/HeadlessAiProfileUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
