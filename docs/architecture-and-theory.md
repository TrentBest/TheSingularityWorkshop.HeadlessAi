# Architecture and theory

HeadlessAi is a reusable .NET transport-and-adapter layer for invoking configured HTTP endpoints. It is not a model, an autonomous operating system, or a universal provider schema.

## Agent configuration model

- **Profile:** immutable endpoint URI, HTTP method, default headers, timeout, maximum response size, and provider-specific non-secret settings.
- **Adapter:** endpoint-specific request construction and response extraction. Built-ins cover OpenAI Responses, Gemini generateContent, and Anthropic Messages; raw-text, delegate, and JSON adapters support custom endpoints.
- **Template:** reusable combination of profile, adapter, and optional request-time header provider.
- **Agent instance:** lightweight caller created from a template and an injected HttpClient. Instances do not own the transport or implicitly share conversation state because conversation state belongs to the host.

This split allows a host to configure a provider endpoint once, then create many agent instances from that configuration. Templates share their adapter and header provider, so those implementations should be stateless or thread-safe. The host may share a single HttpClient across templates and agents.

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
3. Credentials are resolved at invocation time when the host supplies a header provider; HeadlessAi does not log credentials, prompts, or responses.
4. Successful response bodies and error excerpts are bounded.
5. Cancellation and timeouts are correctness properties.
6. Model output is untrusted data, never authority to execute tools or mutate a runtime.
7. Token reduction, lower drift, and accuracy gains are measured hypotheses, not promises.

Keep the core small and add abstractions only where a real provider or host scenario proves the need. Prefer explicit extension points and measured performance over reflection-heavy discovery, hidden caches, or unnecessary JSON round-trips.

## Optional ProtocolAi + GrammarAi funnel

The intended hypothesis is that semantic identifiers plus structural composition can reduce repeated explanatory tokens and ambiguity. This is not guaranteed: a model may misunderstand a compact representation. Hosts need versioned schemas, validation, bounded repair behavior, and reproducible comparisons with a natural-language baseline.
