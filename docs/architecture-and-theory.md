<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

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

---

<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://github.com/TrentBest.png?size=200" alt="The Singularity Workshop on GitHub" width="200">
  </a>
</p>

<p align="center">
  <a href="https://github.com/TrentBest">GitHub</a> ·
  <a href="https://coderlegion.com/">Coder Legion</a> ·
  <a href="https://www.patreon.com/c/TheSingularityWorkshop">Patreon</a> ·
  <a href="https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J">PayPal</a>
  <br>
  <a href="https://github.com/TrentBest/FSM_API">FSM_API</a> ·
  <a href="https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization">FSM_Serialization</a> ·
  <a href="https://github.com/TrentBest/WebPage">WebPage</a>
</p>

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong><br><em>And because static boundaries are invitations to cause trouble.</em></p>
