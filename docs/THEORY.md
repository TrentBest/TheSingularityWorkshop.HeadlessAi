<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# HeadlessAi Theory

> **The theory explains why the boundary exists. Architecture describes how the implementation enforces it.**

## 1. A headless agent is an invocation capability

A configured HTTP caller can be useful to many applications without becoming an application itself. The useful primitive is not “an AI that can do anything”; it is a bounded capability to send a well-defined request to a configured endpoint and obtain a bounded response.

That distinction lets a host create many logical workers while retaining control of task meaning, scheduling, permissions, memory, budgets, and whether any result is accepted.

## 2. Separate identity, protocol, transport, and authority

These are different concerns:

- **Identity/configuration:** which endpoint and model settings are intended.
- **Protocol:** how that endpoint represents requests, responses, options, and errors.
- **Transport:** how the request is sent, cancelled, timed out, and bounded.
- **Authority:** what the application is allowed to do with the returned information.

Combining them creates hidden coupling. Keeping them separate allows an adapter to change without changing the host's permission policy, or the host's task roles to change without inventing a new transport stack.

## 3. A small common core, explicit differences at the edge

The common core should standardize only concepts genuinely shared across endpoints. Endpoint-specific schemas remain explicit at adapters. This is deliberate asymmetry: a small stable center with replaceable protocol edges.

A lowest-common-denominator request format may look simpler but can hide meaningful differences in system instructions, content blocks, generation options, token usage, tool semantics, and error handling. It also tends to become a bag of optional fields that fails to model any provider well.

## 4. Many logical callers do not require many transports

Logical agents are software identities and calling contexts. They do not each need their own HttpClient, socket pool, or thread. A host can create many callers from a shared template and reuse an injected transport. This reduces avoidable resource duplication, but it does not by itself solve provider rate limits or concurrency control.

A production host must still define maximum in-flight requests, queue behavior, per-agent budgets, fairness, cancellation policy, and shutdown behavior.

## 5. Compact protocols are a hypothesis about repeated information

When many tasks reuse the same vocabulary and structural rules, sending those definitions repeatedly may be wasteful. ProtocolAi and GrammarAi can potentially separate stable meaning and structure from the per-task payload.

The benefit depends on amortization. Setup definitions cost tokens; compact symbols may be harder for a model to interpret; output and repair costs may change; providers may offer caching that alters the economics. Measure the entire workflow over a representative set of repeated tasks.

## The invocation lifecycle is a sequence of separate decisions

A useful mental model is not simply “send prompt, receive answer.” The full path has distinct boundaries:

```text
host intent and configuration
        |
        v
endpoint adapter maps request
        |
        v
bounded HTTP invocation
        |
        v
adapter extracts response content
        |
        v
host checks protocol / structure / meaning
        |
        v
host policy decides what happens next
```

HeadlessAi is responsible for the middle transport-and-adapter boundary: mapping the request for the selected endpoint, performing the configured HTTP call, respecting timeout/cancellation/response limits, and extracting content according to the adapter contract. It does not decide whether that content answers the right question, whether the user is authorized to act on it, or whether a proposed action is safe.

Failure is a first-class path, not an exception to the model. A call can fail before sending, during transport, at the HTTP status boundary, while parsing an endpoint response, or because a configured bound is exceeded. The host should distinguish retryable from non-retryable failures according to its own operation and budget; a retry is not automatically safe for every endpoint or side effect.

The response and the failure path both return control to the host. Neither should be silently converted into trusted intent. The host decides whether to retry, request clarification, display the result, reject it, or continue through additional validation.

## 6. Structure is not truth

A grammar can make an answer syntactically valid without making it correct. A protocol can make identifiers stable without ensuring the model interprets them correctly. Validation should therefore have multiple layers:

1. **Transport validity:** did the HTTP invocation succeed?
2. **Protocol validity:** did the response match the endpoint's expected shape?
3. **Structural validity:** does output satisfy the required grammar or schema?
4. **Semantic validity:** does the result satisfy the task's meaning and constraints?
5. **Policy validity:** is the host allowed to act on this result?

HeadlessAi principally handles transport and adapter-level protocol extraction. The host or explicit validator owns the higher-level checks.

## 7. Untrusted output cannot grant itself authority

A response may contain instructions, code, tool arguments, URLs, or proposed mutations. Those are data. They do not grant permission. The host must separately authorize any tool call, external side effect, live runtime mutation, or consequential action.

This invariant applies even when the response comes from a trusted vendor or a well-performing model.

## 8. Performance is a property to measure

Design can avoid obvious overhead: duplicate clients, redundant serialization, unbounded buffering, and unnecessary object graphs. But design intent is not a benchmark result. Measure allocations, throughput, latency, payload size, and resource behavior under documented conditions.

Likewise, token savings and drift reduction are not properties the package can promise by naming alone. They require reproducible experiments with total cost, quality, and failure rates reported.

## Consequences for design

- Inject the HTTP transport.
- Keep profiles immutable and templates reusable.
- Make adapter contracts explicit and fixture-testable.
- Bound time and response size.
- Keep credentials out of ordinary logs and source control.
- Keep ProtocolAi and GrammarAi optional.
- Keep orchestration, memory, permissions, and consequential actions outside this library.
- Prefer small, evidenced abstractions over speculative frameworks.

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
