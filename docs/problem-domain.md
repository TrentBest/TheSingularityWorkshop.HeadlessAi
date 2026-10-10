# The Headless Agent Problem Domain

> **Purpose:** define the problem HeadlessAi exists to solve, the domain it owns, the concepts it names, and the boundaries it must preserve.

## The problem

Applications increasingly need to ask one or many remote AI endpoints to perform bounded pieces of work. The caller may be a desktop application, service, browser-backed host, automation system, or TheForge. The caller should not need to embed a provider SDK, duplicate HTTP plumbing for every agent, or pretend every provider accepts the same JSON.

A useful application may configure one endpoint and create many logical workers from it: researchers, reviewers, classifiers, translators, validators, summarizers, or task-specific agents. These are application-defined roles, not special powers granted by HeadlessAi. Each worker still needs an endpoint contract, a request, bounded transport behavior, and a result the host can evaluate.

Without a deliberate boundary, applications accumulate provider-specific HTTP code, duplicated headers and settings, fragile schema assumptions, unbounded response handling, credential mistakes, and unmeasured claims about reliability or token savings.

HeadlessAi centralizes the reusable HTTP-agent invocation seam while leaving each application's domain and authority where they belong.

## Domain definition

**HeadlessAi is a .NET library for configuring and invoking AI-capable HTTP endpoints through explicit, replaceable protocol adapters.**

It is not itself a model, an autonomous operating system, an orchestration engine, a prompt framework, a memory store, a GUI, or a permission system. It is the headless transport-and-adapter layer between a host's work contract and a configured remote endpoint.

The name follows the Workshop convention: Ai indicates that the package touches AI workflows; it does not claim that the package itself is intelligent.

## The domain vocabulary

| Concept | Meaning | Does not mean |
|---|---|---|
| Endpoint profile | Reusable target URI, HTTP method, safe defaults, timeout, response bound, and provider-specific non-secret settings. | A credential vault or a guarantee that the endpoint is trustworthy. |
| Adapter | Explicit mapping between normalized input/output and one endpoint's request/response protocol. | A universal schema imposed on all providers. |
| Agent template | Reusable combination of profile, adapter, and optional request-time header provider. | A running autonomous process. |
| Agent instance | Lightweight caller created from a template and host-provided HTTP transport. | A dedicated thread, socket pool, model, memory, or execution loop. |
| Input | Task content and optional host-supplied metadata for one invocation. | Authorization to access arbitrary tools or data. |
| Output | Extracted response content plus optional provider metadata. | Proof of correctness, truth, or safe-to-execute instructions. |
| Host | Application that owns task planning, memory, permissions, scheduling, budgets, validation, and consequential actions. | A dependency HeadlessAi should absorb. |
| Provider protocol | The request, response, authentication, option, and usage conventions of a specific endpoint. | A detail that can safely be hidden behind a lowest-common-denominator body. |

## The invocation lifecycle

~~~mermaid
flowchart TD
    A[Host defines bounded task] --> B[Select endpoint profile]
    B --> C[Select provider adapter]
    C --> D[Resolve request-time headers]
    D --> E[Construct endpoint-specific request]
    E --> F[Send through injected HTTP transport]
    F --> G{HTTP success?}
    G -- No --> H[Bounded error result or exception]
    G -- Yes --> I[Bound response body]
    I --> J[Adapter extracts content and metadata]
    J --> K[Host validates meaning and policy]
    K --> L[Host accepts, retries, or acts]
~~~

HeadlessAi is responsible for transport and adapter behavior. It must not turn a response into an executable tool call or mutate the host's live state by itself.

## Why many agents can share one endpoint definition

A template is reusable configuration; an agent instance is a convenient invocation surface. The template can create many lightweight callers without each caller recreating provider settings or owning an independent network connection pool. The host can assign distinct roles and task contexts while retaining one explicit endpoint contract.

Concurrency limits, queueing, rate budgets, per-role permissions, and shared conversation state belong to the host or a deliberately introduced coordination component. Creating more agent instances is not permission to send unlimited parallel requests.

## Provider diversity is a first-class fact

Providers can differ in endpoint paths and model naming, authentication and API-version requirements, system-instruction placement, message roles, content-block shapes, generation settings, tool semantics, structured output, streaming, multimodal payloads, error envelopes, and token accounting.

Therefore, the shared contract should be small, while provider adapters remain explicit and testable. An abstraction is good when it removes duplicated mechanics without erasing meaningful differences.

The built-in OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters cover an initial text-only vertical slice. Their existence does not imply support for every feature those APIs expose. See [Provider adapters](provider-adapters.md).

## The ProtocolAi + GrammarAi hypothesis

ProtocolAi and GrammarAi may optionally help a host represent repeated meaning compactly and constrain message structure. HeadlessAi does not require either package and remains useful when both are absent.

The hypothesis is that a stable vocabulary plus a reusable grammar can reduce repeated explanatory tokens and some classes of ambiguity. This is not a theorem. A model can misunderstand compact symbols, a valid structure can contain false claims, and setup tokens can outweigh savings for small workloads.

Full cost includes vocabulary and grammar definitions, request and output tokens, cache usage and pricing where applicable, repair attempts, and human correction. The [measurement plan](measurement-plan.md) defines comparisons that can support or reject the hypothesis. Claims of eliminating hallucinations or drift are not acceptable without evidence; report measured rates and limitations, not absolutes.

## Trust and authority

The remote response is untrusted input. The host must validate it against the task's expected result shape and policy before use. If a response proposes a code change, tool call, file edit, purchase, deployment, or runtime mutation, that proposal remains inert until the host's own authorization path accepts it.

Credentials should be resolved through host-owned secret storage or a request-time header provider. Avoid putting secrets in checked-in settings, diagnostics, benchmark results, or ordinary logs.

Direct HTTP does not bypass provider authentication, billing, rate limits, endpoint allowlists, or network policy. It is a transport choice, not a privilege escalation mechanism.

## Domain boundaries

- **HeadlessAi:** endpoint invocation, protocol adapters, normalized input/output, cancellation/timeouts, response bounds, and transport extension points.
- **ProtocolAi:** optional semantic identity and compact protocol vocabulary.
- **GrammarAi:** optional constrained structural representation.
- **TheForge / host:** task planning, agent roles, task queues, state, validation, permissions, approvals, budgets, and visible execution.
- **FSM_COS:** runtime composition, when the host uses it.
- **FSM_API:** finite-state-machine runtime behavior, when the host uses it.
- **Provider:** model inference and the provider's endpoint contract.

HeadlessAi must not depend upward on TheForge or FSM_COS. Integration must remain possible from a small application that only wants direct HTTP calls.

## Non-goals

The first package line does not promise an autonomous planning loop, persistent conversation history, a universal memory model, arbitrary URL crawling, browser automation, universal multimodal or streaming support, provider-neutral tool calling where semantics differ, built-in secret storage, global rate limiting, cost accounting, guaranteed correctness, hallucination elimination, or token savings.

These can be investigated as separate capabilities when there is a clear contract, a testable need, and an appropriate ownership boundary.

## Acceptance criteria for the domain

A change fits HeadlessAi when it improves explicit endpoint invocation without stealing authority from the host, obscuring provider differences, adding avoidable dependencies, or making unmeasured performance promises.

Every new capability should answer:
1. Which endpoint or host problem does this solve?
2. Which package owns the contract?
3. How is the failure path represented?
4. Can it be tested deterministically without a live endpoint or secret?
5. What does it cost in allocations, latency, complexity, and tokens?
6. What must the host still validate or authorize?
