# ✳️ 00 TheSingularityWorkshop.HeadlessAi

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/LICENSE)
[![Build](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml/badge.svg?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml)
[![Code Coverage](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi/branch/development/graph/badge.svg)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.HeadlessAi/development/docs/assets/headless-ai-architecture.svg" alt="The host owns task intent, validation, permissions, memory, and actions; HeadlessAi constructs bounded HTTP requests through explicit protocol adapters; configured endpoints perform inference." width="100%">
</p>
<p align="center"><em>Many logical callers. Explicit endpoint protocols. Host-owned authority.</em></p>

## 🟦 01 What is HeadlessAi?

**HeadlessAi gives a .NET application a headless input-to-response connection to a configured AI-capable HTTP endpoint.** The host supplies the input; HeadlessAi sends the request and returns extracted response content. No browser window, GUI automation, or provider SDK is required for the current direct-HTTP path.

The core contracts should not require a particular LLM identity, model class, or provider SDK. An adapter handles the endpoint's protocol; the host decides what the returned content means and what may happen next.

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

HeadlessAi should remain independently useful. Optional Workshop integrations must not become prerequisites for a simple HTTP invocation.

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

The current source includes raw-text, delegate, JSON, OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters. These are concrete protocol adapters around generic invocation contracts—not evidence that the core domain should depend on provider or model types.

## 🟢 04 See it in a minute

The current public API uses constructors for the profile and template; a fluent endpoint builder is a design target, not shipped functionality. This source-shaped example uses one implemented protocol, OpenAI Responses. It makes a live provider request, so set a real authorized API key and replace the example model identifier before running.

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

Expected behavior: the program prints the response text extracted by the selected adapter. The example has been checked against current source contracts but has not been run against a live provider. For prerequisites, provider setup, transport limits, and build/test commands, use the [Usage Guide](docs/usage.md).

## Ecosystem fit and maturity

HeadlessAi can be used on its own by any .NET host that needs its HTTP invocation boundary. It does not require TheForge, FSM_COS, FSM_API, FSM_REST, ProtocolAi, or GrammarAi. FSM_REST may help an editor or authoring tool create endpoint recipes, but that is separate from HeadlessAi runtime dependencies.

ProtocolAi and GrammarAi are optional companions. Streaming, rich multimodal content, provider-specific tool semantics, global rate/cost budgets, and a host orchestration loop are not promised by this package. Direct HTTP does not bypass provider authentication, billing, rate limits, or access policies.

Local benchmarks use deterministic fixture HTTP responses rather than live provider calls. Benchmark numbers should be reported only after running the benchmark and recording environment and methodology. NuGet publication remains disabled in CI unless the repository owner explicitly approves a release.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong><br><em>And because static boundaries are invitations to cause trouble.</em></p>
