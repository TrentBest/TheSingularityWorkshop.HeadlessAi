<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>


# ✳️ 00 TheSingularityWorkshop.HeadlessAi

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/LICENSE)
[![Build](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml/badge.svg?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml)
[![Code Coverage](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi/branch/development/graph/badge.svg)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.HeadlessAi/development/docs/assets/headless-ai-architecture.svg" alt="The host owns task intent, validation, permissions, memory, and actions; HeadlessAi constructs bounded HTTP requests through explicit protocol adapters; configured endpoints perform inference." width="100%">
</p>
<p align="center"><em>Many logical callers. Explicit endpoint protocols. Host-owned authority.</em></p>

## 🟣 01 Why use HeadlessAi?

Connecting an application to an AI endpoint can pull provider-specific HTTP details into code that should be focused on the application's job. HeadlessAi separates endpoint configuration and protocol adaptation from the host's own policy, memory, permissions, and actions.

Use it when a .NET application needs a direct input-to-response HTTP path without browser automation or a provider SDK in the core integration. The host still decides what the response means and what may happen next.

**Explore the Workshop:** [Find packages on NuGet](https://www.nuget.org/packages?q=TheSingularityWorkshop) · [Browse the repository guide](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/WORKSHOP_REPOSITORIES.md)

## 🟦 02 What is HeadlessAi?

**HeadlessAi gives a .NET application a headless input-to-response connection to a configured AI-capable HTTP endpoint.** The host supplies the input; HeadlessAi sends the request and returns extracted response content. No browser window, GUI automation, or provider SDK is required for the current direct-HTTP path.

The core contracts do not require a particular LLM identity, model class, or provider SDK. An adapter handles the endpoint's protocol; the host decides what the returned content means and what may happen next.

This is API communication, not browser automation. A provider's API is not necessarily the same interface as its public chat website, and a URL alone does not make different protocols interchangeable.

## 🟣 03 Start using HeadlessAi

- **Run a real request:** [Usage guide](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/usage.md) — current constructor-based API, endpoint configuration, credentials, response limits, and failure behavior.
- **Understand the boundary:** [What is HeadlessAi?](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/WHAT_IS_HEADLESSAI.md).
- **See the moving parts:** [Gallery](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/GALLERY.md).
- **Browse related tools:** [The Singularity Workshop repository guide](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/WORKSHOP_REPOSITORIES.md).

## 🩵 04 The problem and solution in depth

### One configuration, reusable callers

The current API separates four responsibilities:

| Part | What it means | What it does not mean |
|---|---|---|
| **Profile** | Immutable endpoint URI, HTTP method, headers, timeout, response bound, and provider-specific non-secret settings. | A secret vault or proof that an endpoint is trustworthy. |
| **Adapter** | Explicit mapping to and from one endpoint's protocol. | A universal schema for every provider. |
| **Template** | Reusable profile, adapter, and optional request-time header provider. | A running autonomous process. |
| **Agent instance** | Lightweight caller using an injected `HttpClient`. | A dedicated thread, connection pool, model, memory, or permission grant. |

The host can configure a target and create many logical callers while reusing its HTTP transport. It still owns task planning, concurrency, cost budgets, role permissions, conversation state, output validation, and approval of consequential actions.

The current source includes raw-text, delegate, JSON, OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters. These are concrete protocol adapters around generic invocation contracts; the core domain remains independent of provider and model types.

## 🟢 05 See it in a minute

The current public API uses constructors for the profile and template. This example uses OpenAI Responses as one concrete protocol; it makes a live provider request, so set an authorized API key and replace the example model identifier before running.

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TheSingularityWorkshop.HeadlessAi;

var profile = new HeadlessAiProfile(
    id: "research",
    endpoint: new Uri("https://api.openai.com/v1/responses"),
    settings: new Dictionary<string, string>
    {
        ["model"] = "your-model",
        ["instructions"] = "Answer in one concise paragraph.",
        ["max_output_tokens"] = "256"
    });

using var httpClient = new HttpClient();
var template = new HeadlessAiAgentTemplate(
    profile,
    new OpenAiResponsesAdapter(),
    headerProvider: new EnvironmentBearerHeaderProvider());

var caller = template.CreateAgent(httpClient);
var result = await caller.SendAsync(
    new HeadlessAiInput("Explain what an HTTP endpoint is."));
Console.WriteLine(result.Content);

sealed class EnvironmentBearerHeaderProvider : IHeadlessAiRequestHeaderProvider
{
    public ValueTask<IReadOnlyDictionary<string, string>> GetHeadersAsync(
        HeadlessAiProfile profile, CancellationToken cancellationToken = default)
    {
        var token = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Set OPENAI_API_KEY first.");

        IReadOnlyDictionary<string, string> headers =
            new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" };
        return ValueTask.FromResult(headers);
    }
}
```

Expected behavior: the program prints the response text extracted by the selected adapter. The example has been checked against current source contracts but has not been run against a live provider. For prerequisites, provider setup, transport limits, and build/test commands, use the [Usage Guide](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/usage.md).

## 🟪 06 Documentation and theory

- **[Usage Guide](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/usage.md)** — configure the current constructor-based API, call an endpoint, and understand credentials, limits, and failure behavior.
- **[What is HeadlessAi?](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/WHAT_IS_HEADLESSAI.md)** — build the mental model without needing to know the implementation first.
- **[Theory](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/THEORY.md)** — understand why endpoint protocol, transport, identity, and application authority are separate concerns.
- **[Problem Domain](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/problem-domain.md)** — understand the request, response, and host responsibilities.
- **[Architecture](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/architecture-and-theory.md) and [Dependency Boundaries](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/architecture-boundaries.md)** — inspect responsibility ownership and prohibited dependencies.
- **[Provider Adapters](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/provider-adapters.md) and [Provider Failure Contract](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/provider-failure-contract.md)** — see supported direct-HTTP protocol slices and the limits of their parsing behavior.

- **[Security](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/security.md), [Performance](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/performance.md), and [Measurement Plan](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/measurement-plan.md)** — review trust boundaries and what evidence is required before making claims.
- **[Documentation Index](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/DOCUMENTATION_INDEX.md)** — navigate the full guide set.

## Ecosystem fit and maturity

HeadlessAi can be used on its own by any .NET host that needs its HTTP invocation boundary. It does not require TheForge, FSM_COS, FSM_API, FSM_REST, ProtocolAi, or GrammarAi. FSM_REST may help an editor or authoring tool create endpoint recipes, but that is separate from HeadlessAi runtime dependencies.

ProtocolAi and GrammarAi are optional companions. Streaming, rich multimodal content, provider-specific tool semantics, global rate/cost budgets, and a host orchestration loop are not promised by this package. Direct HTTP does not bypass provider authentication, billing, rate limits, or access policies.

Local benchmarks use deterministic fixture HTTP responses rather than live provider calls. Benchmark results depend on the environment and methodology; see the benchmark guide for interpretation.

---

## 🔗 Resources, code & support

### 📦 Packages and source

- **HeadlessAi:** [NuGet package page](https://www.nuget.org/packages/TheSingularityWorkshop.HeadlessAi) · [Source repository](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi)
- **FSM_API:** [Core NuGet package](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API) · [Unity Asset Store](https://assetstore.unity.com/packages/slug/332450) · [Source repository](https://github.com/TrentBest/FSM_API)
- **FSM_COS:** [NuGet package](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_COS) · [Source repository](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS)
- **FSM_Serialization:** [NuGet package](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_Serialization) · [Source repository](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization)
- **WebPage:** [Source repository](https://github.com/TrentBest/WebPage)
- **FSM_API_Unity:** [Source repository](https://github.com/TrentBest/FSM_API_Unity)

### 🌐 Find the Workshop

- **GitHub:** [The Singularity Workshop repositories](https://github.com/TrentBest)
- **CoderLegion:** [Join the Workshop community](https://coderlegion.com/user/The+Singularity+Workshop)

### 💖 Support The Singularity Workshop

- **Patreon:** [Support the Workshop](https://www.patreon.com/c/TheSingularityWorkshop)
- **PayPal:** [Make a donation](https://www.paypal.com/donate/?hosted_button_id=3Z7263LCQMV9J)

<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar and brand" width="200">
  </a>
</p>

<p align="center">
  <a href="https://github.com/TrentBest/WebPage" aria-label="Visit The Singularity Workshop"><img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex of information converging on its central eye" width="112" /></a><br />

  <em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br>
  <strong>Because state shouldn't be a mess.</strong><br>
  <em>And because static boundaries are invitations to cause trouble.</em>

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
