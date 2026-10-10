# ✳️ 00 TheSingularityWorkshop.HeadlessAi

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/LICENSE)
[![Build](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml/badge.svg?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml)
[![Code Coverage](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi/branch/development/graph/badge.svg)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.HeadlessAi/development/docs/assets/headless-ai-architecture.svg" alt="The host owns task intent, validation, permissions, memory, and actions; HeadlessAi constructs bounded HTTP requests through explicit protocol adapters; configured endpoints perform inference." width="100%">
</p>
<p align="center"><em>Many logical callers. Explicit endpoint protocols. Host-owned authority.</em></p>

## 🟦 01 What is HeadlessAi?

**HeadlessAi lets a .NET application send input to a configured AI-capable HTTP endpoint and receive its response, without a browser or GUI.**

The core provides a small, replaceable HTTP invocation boundary. It does not need a type for any particular LLM or a provider SDK. A protocol adapter handles endpoint-specific request and response formats; the host decides what the returned content means and what may happen next.

This is API communication, not browser automation. A provider's API is not necessarily the same interface as its public chat website, and a URL alone does not make different protocols interchangeable.

## 🟣 02 The Workshop documentation map

This repository follows The Singularity Workshop's shared [Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md), whose goal is to **edify, not mystify**: explain the problem, orient the reader, show a credible first proof, and link to deeper explanations.

- **New to the idea?** Start with [What is HeadlessAi?](docs/WHAT_IS_HEADLESSAI.md).
- **Want to use the current API?** Follow the [Usage Guide](docs/usage.md).
- **Want to understand why the boundary exists?** Read [Theory](docs/THEORY.md) and [Problem Domain](docs/problem-domain.md).
- **Need implementation ownership and limits?** Read [Architecture](docs/architecture-and-theory.md) and [Architecture Boundaries](docs/architecture-boundaries.md).
- **Want the intended fluent configuration direction?** Read [Fluent Configuration Design](docs/fluent-configuration-design.md). This is a proposal, not a shipped API.
- **Want to see where it fits across the Workshop?** Read [Ecosystem Integration](docs/ecosystem-integration.md), which separates current behavior from proposed uses.
- **Want the full document map?** Open the [Documentation Index](DOCUMENTATION_INDEX.md).

HeadlessAi has its own domain and vocabulary. It should remain independently useful; optional Workshop integrations must not become prerequisites for a simple HTTP invocation.

## 🩵 03 The problem and solution in depth

### One configuration, reusable callers

The current API separates four responsibilities:

| Part | What it means | What it does not mean |
|---|---|---|
| **Profile** | Immutable endpoint URI, HTTP method, headers, timeout, response bound, and provider-specific non-secret settings. | A secret vault or proof that an endpoint is trustworthy. |
| **Adapter** | Explicit mapping to and from one endpoint's protocol. | A universal schema for every provider. |
| **Template** | Reusable profile, adapter, and optional request-time header provider. | A running autonomous process. |
| **Agent instance** | Lightweight caller using an injected `HttpClient`. | A dedicated thread, connection pool, model, memory, or permission grant. |

The host can configure a target and create many logical callers while reusing its HTTP transport. It still owns task planning, concurrency, cost budgets, role permissions, conversation state, output validation, and approval of consequential actions.

### What is implemented—and what is not

The current source includes raw-text, delegate, JSON, OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters. These built-ins are an initial text-focused slice, not full support for every feature of those services. Concrete adapters speak protocols; they do not make provider or model identity part of HeadlessAi's core domain.

The fluent endpoint-configuration API is a **design target, not currently shipped**. The current usage guide documents the constructor-based public API. The [design note](docs/fluent-configuration-design.md) records the target experience and the tests it must pass before it can be presented as available.

ProtocolAi and GrammarAi are optional companions, not dependencies of this package. Their possible value in repeated structured tasks is a testable hypothesis, not a package guarantee. The host must validate meaning and must never treat model output as authority to execute tools or modify state.

## 🟢 04 See it in a minute

The [Usage Guide](docs/usage.md) provides the current source-shaped example, prerequisites, and build/test commands. Its concrete provider example uses OpenAI Responses because that adapter is implemented; that choice is an example, not the definition of HeadlessAi. The example makes a live network request and has not been verified against a live provider. Use authorized credentials and keep secrets out of source control.

Local benchmarks use deterministic fixture HTTP responses rather than live provider calls. Benchmark numbers should be reported only after running the benchmark and recording environment and methodology.

## Ecosystem fit and maturity

HeadlessAi can be used on its own by any .NET host that needs its HTTP invocation boundary. It does not require TheForge, FSM_COS, FSM_API, FSM_REST, ProtocolAi, or GrammarAi. FSM_REST may be useful to an editor or authoring tool that creates endpoint recipes, but that is separate from HeadlessAi runtime dependencies.

Streaming, rich multimodal content, provider-specific tool semantics, global rate/cost budgets, and a host orchestration loop are not promised by this package. Direct HTTP does not bypass provider authentication, billing, rate limits, or access policies.

NuGet publication remains disabled in CI unless the repository owner explicitly approves a release.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong><br><em>And because static boundaries are invitations to cause trouble.</em></p>
