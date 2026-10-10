# HeadlessAi Across The Singularity Workshop

> **Purpose:** show where HeadlessAi fits in the wider Workshop, how existing packages can cooperate with it, and which integrations are demonstrated, proposed, or deliberately out of scope.

This is an integration map, not a claim that every connection below is already implemented. It distinguishes the package's current capabilities from architectural opportunities so readers can see the intended breadth without mistaking a design sketch for working software.

## 01 — The shortest explanation

HeadlessAi is the **provider-invocation boundary**. It sends a bounded request to a configured AI-capable HTTP endpoint and returns extracted content and metadata. It does not decide what the answer means, whether it is safe, or what the application should do with it.

~~~mermaid
flowchart LR
    U[User / event / workflow] --> H[Host application]
    H -->|task + profile| A[HeadlessAi]
    A -->|provider-specific HTTP| P[Configured AI endpoint]
    P --> A
    A -->|content + metadata| H
    H --> V[Validate meaning and permissions]
    V -->|approved intent only| X[Application behavior]
~~~

The central design rule is:

**HeadlessAi can help a system think about a task; the host remains responsible for deciding and acting.**

A host can use HeadlessAi without adopting any other Workshop package. The packages below are optional ways to make that host more capable.

## 02 — Current evidence versus integration opportunities

| Repository or package | Relationship to HeadlessAi | Status |
|---|---|---|
| [HeadlessAi](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi) | Provider-neutral invocation core, endpoint profiles, reusable templates, HTTP adapters and bounded response handling. | **Implemented in this repository**; alpha candidate, not a release guarantee. |
| [HeadlessAi.AnyAppExample](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi.AnyAppExample) | Intended home for a desktop/application integration example. | **Scaffold only at the time of review**: its README contains only a title and its root contains no visible example project yet. Treat it as a follow-up, not proof of a working integration. |
| [HeadlessAi.Benchmarks](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi.Benchmarks) | Intended home for a dedicated benchmark consumer. | **Scaffold only at the time of review**: its README is a placeholder and the repository root has no visible benchmark project yet. HeadlessAi itself already contains a benchmark project; use that until the companion repo is implemented. |
| [ProtocolAi](https://github.com/TrentBest/TheSingularityWorkshop.ProtocolAi) | Gives application-owned concepts deterministic identities. Can resolve model-returned names or symbols after invocation. | **Compatible, optional host-side integration**; not a HeadlessAi core dependency. |
| [GrammarAi](https://github.com/TrentBest/TheSingularityWorkshop.GrammarAi) | Defines allowed relationships and structure among ProtocolAi identities. Can help a host check whether a proposed response has an admitted shape. | **Compatible, optional host-side integration**; not a substitute for truth, authorization, or execution policy. |
| [FSM_API](https://github.com/TrentBest/FSM_API) | Represents application state and state-driven workflows that may decide when to ask a model and what to do with its answer. | **Architectural fit**; HeadlessAi does not require FSM_API. |
| [FSM_UserIO](https://github.com/TrentBest/FSM_UserIO) | Carries platform-neutral semantic intent across an input/intent boundary. A host may translate a validated model proposal into an intent for normal application review. | **Architectural fit**; the host must retain validation and authority. |
| [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS) | Composes a runtime from capabilities. A host could use AI-assisted authoring or diagnosis around a composition request. | **Host-level opportunity only**; HeadlessAi is not a composition engine and must not be required by FSM_COS core. |
| [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain) | Defines neutral contracts for capabilities. An AI-backed capability could use HeadlessAi internally while exposing its own domain contract. | **Possible MicroBundle implementation**; the domain contract should not inherit provider-specific AI concerns. |
| [MicroBundleRepository](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository) | Stores and delivers versioned artifacts. AI could help authors draft metadata or descriptions, but artifact identity and verification stay deterministic. | **Optional tooling opportunity**, not a repository runtime dependency. |
| [Experiences](https://github.com/TrentBest/TheSingularityWorkshop.Experiences) | Provides Experience manifests and artifacts consumed by different manifestations. AI may assist creation, explanation, or user-directed customization before an artifact is accepted. | **Host/tooling opportunity**; model output must never become trusted executable content just because it produced a manifest. |
| [WebApp](https://github.com/TrentBest/TheSingularityWorkshop.WebApp) and [WebPage](https://github.com/TrentBest/WebPage) | Browser-facing discovery, learning, authoring, and experience surfaces can offer opt-in AI-assisted help. | **Potential host integrations**, not evidence of a shipped AI feature. |
| [MyVR](https://github.com/TrentBest/TheSingularityWorkshop.MyVR) | Could use AI for conversational guidance, explanation, or authoring assistance while VR tracking, rendering, and interaction remain native responsibilities. | **Future-facing integration idea**; no claim of current integration. |
| [Ontology](https://github.com/TrentBest/TheSingularityWorkshop.Ontology) | A host can map model-suggested concepts to its own semantic addresses, then use ontology relationships to locate candidate behavior or representation. | **Optional consumer-side composition**; Ontology deliberately has no ProtocolAi dependency. |
| [Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer) | AI might help author semantic descriptions, classify content, or propose high-level scene intent. Per-frame rendering and Event Horizon computation should remain deterministic and locally governed. | **Research/tooling opportunity**, not a rendering dependency. |
| [Profiles](https://github.com/TrentBest/Profiles) | A profile-aware host could use AI to draft or summarize profile content, subject to access control and explicit sharing policy. | **Possible application feature**; private data must not be sent to a provider by default. |
| [TheSingularityWorkshop.Economy](https://github.com/TrentBest/TheSingularityWorkshop.Economy) | AI may explain options or help a user compare scenarios; authoritative accounting, prices, balances, and transactions remain ordinary domain logic. | **Possible advisory interface**, never the ledger or authority. |
| [TheSingularityWorkshop.MicroBundleIngestor](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleIngestor) | AI could propose tags, descriptions, or candidate classifications during ingestion. Deterministic validation and explicit approval should gate accepted metadata. | **Possible authoring assistant**, not a trusted ingestion engine. |
| [raWWar](https://github.com/TrentBest/raWWar) | A game or simulation host could ask a model for bounded tactical suggestions, then validate the proposed move against authoritative game state and rules. | **Conceptual fit only**; this document does not imply or change that repository's implementation. |

The status labels matter. “Possible” means the architecture can support the idea; it does not mean an adapter, user interface, or production workflow already exists.

## 03 — Three patterns users can copy

### Pattern A: ask, validate, then act

Use this when an application has a task that benefits from language or probabilistic reasoning, but actions must remain deterministic.

~~~text
FSM_API / event / user request
             |
             v
       Host task policy
             |
             v
         HeadlessAi
             |
             v
        AI endpoint
             |
             v
       Proposed answer
             |
             v
  parse + domain validation
             |
             v
  permission / user approval
             |
             v
      ordinary app logic
~~~

Examples: summarize a document, suggest a route, explain a configuration, propose a game move, or draft a MicroBundle description. The host decides whether the result is accepted, retried, shown as a suggestion, or discarded.

### Pattern B: probabilistic language into deterministic meaning

Use this when the model's answer contains names, actions, or concepts the application already knows.

~~~text
User's words / model response
             |
             v
         HeadlessAi
             |
             v
     candidate text or JSON
             |
             v
         ProtocolAi
     known symbol identity
             |
             v
         GrammarAi
     permitted structure check
             |
             v
     host semantic validation
             |
             v
       host-owned action
~~~

ProtocolAi answers **“what does this known symbol identify?”** GrammarAi answers **“is this arrangement admitted by the defined structure?”** Neither proves that a claim is true or that an action is authorized. This is an application-side composition; neither package needs to depend on HeadlessAi, and HeadlessAi core must not depend on either. A first-party typed companion bridge is a separate optional roadmap item, not part of the current core.

### Pattern C: AI assistance inside a capability

A MicroBundle or other module may privately use HeadlessAi to provide a capability, while exposing a domain-focused contract to the larger runtime.

~~~text
FSM_COS / host composes a capability
                 |
                 v
        Domain-owned capability
          |             |
          |             +--> deterministic rules / local data
          |
          +--> HeadlessAi (optional)
                    |
                    v
               AI endpoint
~~~

This preserves the package boundaries: MicroBundleDomain defines the capability contract; FSM_COS composes; the host executes and manifests; HeadlessAi invokes the configured endpoint. The AI provider should be an implementation detail unless the capability's public contract explicitly needs to expose AI-specific configuration.

## 04 — Concrete Workshop scenarios

### AnyApp: a desktop host and proving ground

AnyApp is a natural place to demonstrate the full request lifecycle because it owns a desktop manifestation and can host an explicit interaction flow. The user should be able to see what was requested, what came back, and what the host accepted. Deterministic tests should exercise success and failure paths without requiring a paid provider call; a live-provider demo should be opt-in and credentialed by the user.

### WebApp / WebPage: explain, explore, and create

A browser experience could offer AI assistance for explaining Workshop concepts, helping a user navigate documentation, or drafting an Experience. The browser host must decide which context is sent, disclose provider use, apply limits, and keep secrets server-side. Model-generated HTML, manifests, MicroBundles, or commands must be treated as untrusted proposals and validated before use.

### ProtocolAi + GrammarAi: make meaning explicit

A response such as `{"action":"inspect","target":"Bob"}` has shape, but not necessarily application meaning. The host can resolve known vocabulary through ProtocolAi, check a structure with GrammarAi, and then apply its own object lookup, access policy, and domain rules. Unknown values remain a host decision: register, ask, reject, or keep literal.

### FSM_API / FSM_UserIO: AI as one input source

A state-driven process can decide when an AI request is appropriate, track a pending request, handle timeout/cancellation/failure, and route a validated result through the same intent boundary used by other inputs. Do not turn a provider response directly into a state transition or executable command without the host's normal checks.

### Ontology / Renderer: propose meaning, not frame-by-frame control

An AI may help label a user-created object or suggest a high-level relationship. The host can map approved concepts to Ontology indices and then select a suitable behavior or rendering representation. The Renderer should not call a language model every frame; high-frequency work belongs to predictable local computation and the Renderer’s own scheduling/observation model.

### Profiles / Economy: advisory assistance without authority

A model can draft a profile biography or explain financial scenarios, but privacy policy determines whether profile data may leave the host. Economic balances, transaction validity, and settlement remain authoritative domain operations. AI output is explanatory or advisory unless a separately designed, auditable workflow authorizes more.

## 05 — Integration rules that apply everywhere

1. **Keep HeadlessAi optional.** Domain packages should not acquire a provider dependency merely because one application uses AI.
2. **Inject and reuse `HttpClient`.** The host owns transport lifetime and deployment policy; do not create one client per logical agent.
3. **Keep credentials outside source and ordinary profiles.** Use a host-owned request-header provider or secret mechanism, and review redirect/SSRF behavior for the deployment.
4. **Bound work.** Configure timeouts and response limits, and have the host enforce concurrency, rate, token, and cost budgets.
5. **Treat output as untrusted input.** Parsing is not validation; a schema or grammar is not proof; a valid answer is not permission.
6. **Validate against current domain state.** Game rules, permissions, profile access, composition requirements, and financial invariants remain owned by the relevant application.
7. **Keep a human or policy gate for consequential changes.** Do not let model text silently install bundles, modify runtime composition, execute commands, publish content, or transfer value.
8. **Test offline first.** Use fake HTTP handlers and fixtures to test successful parsing, malformed output, provider errors, cancellation, limits, and timeouts. Live calls are integration demonstrations, not a substitute for deterministic tests.
9. **Measure rather than promise.** Do not claim lower cost, fewer tokens, improved correctness, or energy savings without a comparable measured workload.
10. **Document maturity honestly.** Label shipped code, tested integrations, proposed use cases, and roadmap items separately.

## 06 — What this package intentionally does not become

HeadlessAi does not replace:

- **ProtocolAi** — deterministic vocabulary and symbol identity.
- **GrammarAi** — structural rules over protocol identities.
- **FSM_API** — state transitions and processing groups.
- **FSM_UserIO** — platform-neutral user/semantic intent contracts.
- **FSM_COS** — runtime composition and arbitration.
- **MicroBundleDomain** — capability contracts and domain boundaries.
- **MicroBundleRepository** — versioned artifact storage and delivery.
- **Ontology** — semantic indexing and relationships.
- **Renderer** — observation-driven representation and rendering computation.
- **The host** — user experience, memory, permissions, policy, validation, budgets, and actions.

The intended result is composability without dependency sprawl: use only the pieces that solve your problem, and let each package keep its own responsibility.

## 07 — Suggested implementation order

For a developer adopting HeadlessAi, a low-risk sequence is:

1. Start with a standalone console or test host and one configured adapter.
2. Add deterministic tests around provider success, failure, cancellation, and output limits.
3. Add host-side semantic validation and explicit user approval for any consequential action.
4. If useful, add ProtocolAi for known identities and GrammarAi for admitted structure.
5. If the application already uses FSM_API or FSM_UserIO, connect the request lifecycle to its existing state/intent boundaries.
6. Only then consider hosting the capability inside AnyApp, WebApp, a MicroBundle, an Experience authoring tool, or another Workshop manifestation.

Do not adopt FSM_COS, ProtocolAi, GrammarAi, or any other Workshop package just to make a simple HTTP request. HeadlessAi is intended to be independently useful.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong></p>
