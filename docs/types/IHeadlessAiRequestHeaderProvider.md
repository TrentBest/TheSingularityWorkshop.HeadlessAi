<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# IHeadlessAiRequestHeaderProvider

## Purpose

Optional request-time source of HTTP headers, commonly used for credentials supplied by the host.

## Declaration and ownership

- **Declaration:** public interface.
- **Source:** [src/TheSingularityWorkshop.HeadlessAi/IHeadlessAiRequestHeaderProvider.cs](src/TheSingularityWorkshop.HeadlessAi/IHeadlessAiRequestHeaderProvider.cs).
- **Related types:** HeadlessAiAgentTemplate; HeadlessAiAgent.
- **Responsibilities:** Lets headers be resolved per invocation instead of stored as secret-bearing profile data.
- **Non-goals:** It is not a credential vault, rotation policy, or guarantee that a header is safe to log.

## Contract notes

The host owns task planning, credentials, permissions, response validation, budgets, and consequential actions. A successful HTTP exchange is not proof that a response is safe or correct.

## Verification

Relevant evidence: [tests/TheSingularityWorkshop.HeadlessAi.Tests/HeadlessAiTests.cs](tests/TheSingularityWorkshop.HeadlessAi.Tests/HeadlessAiTests.cs). This page is source-authored and checked against the current declaration; it is not generated from reflection or compiler AST.

## Related documentation

- [Problem domain](../problem-domain.md)
- [Architecture boundaries](../architecture-boundaries.md)
- [Usage guide](../usage.md)
- [Companion UML and use cases](../UML/IHeadlessAiRequestHeaderProviderUML.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
