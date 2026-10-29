<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# RawTextHeadlessAiAdapter — UML neighborhood and use cases

> **Target:** RawTextHeadlessAiAdapter. This authored static diagram reflects source-level relationships and is not a runtime trace.

## Neighborhood

```mermaid
flowchart TD
  H[Host / HeadlessAiAgent] --> A[RawTextHeadlessAiAdapter]
  A --> I[IHeadlessAiAdapter contract]
  A --> R[HTTP request and normalized response]
  R --> H2[Host validates returned meaning]
```

## Source-backed relationships

- **Target source:** [src/TheSingularityWorkshop.HeadlessAi/RawTextHeadlessAiAdapter.cs](../../src/TheSingularityWorkshop.HeadlessAi/RawTextHeadlessAiAdapter.cs).
- **Related types:** IHeadlessAiAdapter; HeadlessAiProfile; HeadlessAiInput; HeadlessAiOutput.
- **Evidence:** [tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs](../../tests/TheSingularityWorkshop.HeadlessAi.Tests/ProviderAdapterTests.cs).
- **Scope:** authored from named source and test references; not generated from a repository-wide AST or runtime trace.

## Use case — adapt one endpoint protocol

- **Actor:** host application.
- **Goal:** communicate with an endpoint using its declared request and response shape.
- **Trigger:** the host creates a HeadlessAiAgent with a selected adapter and sends an input.
- **Preconditions:** profile settings and endpoint URI match the selected adapter's protocol.
- **Outcome:** the adapter produces an HTTP request and normalizes response content or surfaces a documented failure.
- **Alternative/failure:** invalid settings, malformed response bodies, non-success HTTP status, timeout, or response-bound violations must fail explicitly.
- **Authority boundary:** adapter output is untrusted content; the host owns validation, permissions, approval, and actions.

## Related pages

- [Type guide](../types/RawTextHeadlessAiAdapter.md)
- [Provider adapters](../provider-adapters.md)
- [Shared Workshop documentation standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md)


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
