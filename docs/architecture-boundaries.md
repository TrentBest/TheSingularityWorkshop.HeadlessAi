# Architecture and Dependency Boundaries

> **The useful boundary lets a host call many configured endpoints without making HeadlessAi responsible for the application.**

## Dependency direction

~~~mermaid
flowchart LR
    P[ProtocolAi optional] -. compact meaning .-> H[Host / TheForge]
    G[GrammarAi optional] -. constrained structure .-> H
    H -->|task + endpoint configuration| A[HeadlessAi]
    A -->|endpoint-specific HTTP| E[Configured endpoint]
    E -->|response| A
    A -->|content + metadata| H
    H -->|validate / approve / act| X[Application or runtime]
~~~

The optional integrations describe host-side composition, not package dependencies. HeadlessAi must compile and operate without ProtocolAi, GrammarAi, TheForge, FSM_COS, or FSM_API.

## Ownership matrix

| Responsibility | Owner | Reason |
|---|---|---|
| Endpoint URL, HTTP method, provider request body | Profile + adapter | Endpoint behavior must be explicit and replaceable. |
| Shared HTTP connection pool | Host-injected HttpClient | Reuse transport; avoid one pool per logical agent. |
| Dynamic credential retrieval | Host-owned provider/secret store | Secrets and rotation belong to the deployment boundary. |
| Timeout, cancellation, bounded response body | HeadlessAi transport | Fundamental invocation guarantees. |
| Task planning and decomposition | Host / orchestrator | Planning policy varies by application and role. |
| Agent identity, role, permissions, budgets | Host / orchestrator | A configured caller is not an authority grant. |
| Conversation memory and retention | Host / memory component | Data lifecycle and privacy are application policy. |
| Output schema and semantic validation | Host or explicit validator | Parsed text is not automatically correct. |
| Tool mediation and consequential action | Host | Model output must not execute itself. |
| Compact semantic identifiers | Optional ProtocolAi integration | Not required for basic HTTP invocation. |
| Structured message grammar | Optional GrammarAi integration | Not required for basic HTTP invocation. |
| Runtime composition | FSM_COS, if used by host | HeadlessAi does not assemble application runtime. |
| FSM state transitions | FSM_API, if used by host | HeadlessAi does not become a workflow engine. |

## Performance constraints

- Reuse an injected HttpClient; do not allocate a new one per agent or request.
- Keep the normalized contract small and avoid redundant serialization passes.
- Parse each response body once where possible; do not log and reparse full content.
- Bound response sizes before buffering untrusted bodies.
- Keep benchmarks deterministic and offline with fake HttpMessageHandler responses.
- Measure allocations and latency before adding caches, pools, reflection, or elaborate object graphs.
- Many logical agents do not constitute a concurrency policy. The host must bound simultaneous sends and aggregate provider usage.

## Failure semantics

Failures should remain distinguishable enough for a host to respond correctly: host cancellation, timeout, non-success HTTP status, response over the configured bound, invalid provider response shape, incomplete adapter configuration, and transport/network failure.

Do not turn an error envelope into a successful empty response. Preserve enough provider error context to diagnose failures, but bound excerpts and never include credentials.

## Change discipline

Before adding a cross-cutting abstraction, demonstrate at least two concrete consumers or a compelling provider difference. Before adding a dependency, document the responsibility it provides and why the BCL or an existing Workshop package is insufficient. Every public contract change requires tests, XML documentation, and updates to the relevant theory and usage docs.
