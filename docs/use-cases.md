# Use cases

## TheForge: specialized headless agents

TheForge can configure several endpoint profiles and create role-specific agents such as researcher, planner, critic, implementation assistant, and documentation reviewer. These roles are host-defined, not hard-coded into HeadlessAi. The host owns routing, queues, budgets, permissions, provenance, and human approval.

## Optional ProtocolAi + GrammarAi composition

A host may use ProtocolAi for semantic identity and GrammarAi for constrained structure before sending a request. The hypothesis is fewer repeated explanatory tokens and less ambiguity. Validate it against a provider-matched natural-language baseline; compact protocols can still be misunderstood.

## Other plausible uses

- Batch document processing against different providers or local models.
- Parallel independent reviews with a host-owned synthesis step.
- Cross-provider contract and regression testing.
- Internal HTTP endpoint invocation without a large provider SDK.
- Privacy-aware routing to a locally hosted model.

The package does not guarantee lower token use, fewer hallucinations, determinism, lower costs, or provider availability.
