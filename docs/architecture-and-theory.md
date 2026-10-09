# Architecture and theory

HeadlessAi is a reusable .NET transport-and-adapter layer for invoking configured HTTP endpoints. It is not a model, an autonomous operating system, or a universal provider schema.

## Boundaries

- The host (such as TheForge) owns task planning, agent roles, permissions, memory, budgets, work queues, and whether a result may change anything.
- ProtocolAi can represent semantic identity and stable vocabulary.
- GrammarAi can compose a constrained message structure.
- HeadlessAi invokes the configured endpoint and delegates endpoint-specific serialization and response extraction to an adapter.
- The model/provider owns its endpoint schema and inference behavior.

ProtocolAi and GrammarAi are optional. HeadlessAi must remain useful without either and must not depend on TheForge, FSM_COS, or FSM_API.

## Invariants

1. Endpoint differences are explicit; no universal JSON request body is assumed.
2. Agents share an injected HttpClient; they do not create or dispose a socket pool per request.
3. Credentials and raw prompts/responses are never logged by default.
4. Cancellation and timeouts are correctness properties.
5. Model output is untrusted data, never authority to execute tools or mutate a runtime.
6. Token reduction, lower drift, and accuracy gains are measured hypotheses, not promises.

Keep the core small: profile, normalized input/output, adapter, agent, and transport. Add abstractions only where a real provider or host scenario proves the need. Prefer explicit extension points and measured performance over reflection-heavy discovery, hidden caches, or unnecessary JSON round-trips.

## Optional ProtocolAi + GrammarAi funnel

The intended hypothesis is that semantic identifiers plus structural composition can reduce repeated explanatory tokens and ambiguity. This is not guaranteed: a model may misunderstand a compact representation. Hosts need versioned schemas, validation, bounded repair behavior, and reproducible comparisons with a natural-language baseline.
