# ✳️ 00 TheSingularityWorkshop.HeadlessAi

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/LICENSE)
[![Build](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml/badge.svg?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml)
[![Code Coverage](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi/branch/development/graph/badge.svg)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.HeadlessAi/development/docs/assets/headless-ai-architecture.svg" alt="The host owns task intent, validation, permissions, memory, and actions; HeadlessAi constructs bounded HTTP requests through explicit provider adapters; configured endpoints perform inference." width="100%">
</p>
<p align="center"><em>Many logical callers. Explicit endpoint protocols. Host-owned authority.</em></p>

## 🟦 01 What is HeadlessAi?

**HeadlessAi lets a .NET application call AI services over HTTP without tying the application to one provider's SDK.**

The basic interaction is **prompt in, response out**: your application supplies text, HeadlessAi sends an HTTP request to a configured AI-service endpoint, and your application receives the returned content—without opening a browser or GUI. HeadlessAi reuses the HTTP plumbing and provider-specific request/response mapping so each application does not have to build them again.

This is API communication, not browser automation. A provider's API is not necessarily the same interface as its public chat website, and a URL alone does not make different protocols interchangeable.

**It is the connection to the AI service—not the AI model or an autonomous agent.** Your application remains in control of what the response means, what is permitted, and what happens next. HeadlessAi does not require the rest of The Singularity Workshop; ProtocolAi and GrammarAi are optional companions.

---

## 🟣 02 The Workshop documentation map

This repository follows The Singularity Workshop's shared [Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/DOCUMENTATION_STANDARD.md), maintained in [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md). The standard is designed to **edify, not mystify**: lead with the problem, orient the reader, provide a credible first proof, then link to deeper explanations instead of cramming the entire manual into the README.

- **New to the idea?** Start with [What is HeadlessAi?](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/WHAT_IS_HEADLESSAI.md).
- **Want to use it?** Follow the [Usage Guide](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/usage.md).
- **Want to understand the why?** Read [Theory](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/THEORY.md) and [Problem Domain](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/problem-domain.md).
- **Need exact boundaries?** Read [Architecture](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/architecture-and-theory.md) and [Architecture Boundaries](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/architecture-boundaries.md).
- **Want to see where it fits across the Workshop?** Read [Ecosystem Integration](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/ecosystem-integration.md) for concrete use patterns, candidate package integrations, and clear implemented-versus-proposed labels.
- **Want the full document map?** Open the [Documentation Index](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/DOCUMENTATION_INDEX.md).

HeadlessAi has its own domain and vocabulary. It adopts the Workshop's shared visual and editorial language without inheriting responsibilities that belong to FSM_COS, FSM_API, ProtocolAi, GrammarAi, or a host application.

---

## 🩵 03 The problem and solution in depth

### A profile is not an agent army

HeadlessAi separates four things that are often accidentally coupled:

| Part | What it means | What it does not mean |
|---|---|---|
| **Profile** | Immutable endpoint, HTTP method, non-secret settings, timeout, and response-size bound. | A secret vault or a guarantee that the endpoint is trustworthy. |
| **Adapter** | Explicit mapping to and from one endpoint's protocol. | A fictional universal provider schema. |
| **Template** | Reusable profile, adapter, and optional request-time header provider. | A running autonomous process. |
| **Agent instance** | A lightweight caller using an injected HttpClient. | A dedicated thread, connection pool, model, memory, or permission grant. |

This lets a host configure a provider once and create many logical callers without making one HTTP connection pool per agent. It does **not** remove the need for host-side concurrency limits, provider budgets, role permissions, or output validation.

### Responsibility boundary

- **HeadlessAi owns:** endpoint invocation, adapter extension points, cancellation and timeouts, response bounds, and normalized response extraction.
- **The host owns:** task planning, roles, memory, queues, concurrency and cost budgets, permissions, semantic validation, and approval of consequential actions.
- **The provider owns:** its endpoint schema, authentication requirements, quotas, and inference behavior.
- **ProtocolAi and GrammarAi:** optional companion integration, not dependencies of this package. The core currently exposes the generic input/adapter seam; a first-party, typed ProtocolAi + GrammarAi bridge is not yet shipped. The planned bridge belongs in a separate opt-in project/package **inside this repository** so consumers can choose it without pulling these dependencies into HeadlessAi core or creating another repository. See the [optional integration roadmap](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/roadmap.md#optional-protocolai--grammarai-bridge).

A well-formed response can still be wrong. A valid grammar is not proof of truth, and model output never grants itself permission to execute a tool, change a runtime, or modify external state.

### Implemented behavior versus research hypotheses

The current source includes raw-text, delegate, JSON, OpenAI Responses, Gemini generateContent, and Anthropic Messages adapters. These built-ins are an initial text-focused slice, not full support for every provider feature.

ProtocolAi + GrammarAi may reduce repeated explanatory tokens or some forms of ambiguity in repeated tasks. That is a **testable hypothesis**, not a package guarantee. Setup costs, repair calls, output validity, semantic drift, and unsupported claims must be measured against a comparable baseline. See the [Measurement Plan](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/measurement-plan.md).

---

## 🟢 04 See it in a minute

This concrete example uses **one specific protocol: OpenAI's Responses API**. It is not a HeadlessAi requirement, and the endpoint URL is not interchangeable with other providers' URLs. The selected adapter must match the service's request and response format. This calls the provider API directly; it does not automate the ChatGPT website or reuse a signed-in web session.

It sends a live request, so use your own authorized endpoint and provide a real credential through a host-owned secret mechanism. This example has been checked against the current source contracts but has not been run against a live provider. It is not an offline unit test.

~~~csharp
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

var agent = template.CreateAgent(httpClient);
var result = await agent.SendAsync(new HeadlessAiInput("Explain what an HTTP endpoint is."));
Console.WriteLine(result.Content);

sealed class EnvironmentBearerHeaderProvider : IHeadlessAiRequestHeaderProvider
{
    public ValueTask<IReadOnlyDictionary<string, string>> GetHeadersAsync(
        HeadlessAiProfile profile,
        CancellationToken cancellationToken = default)
    {
        var token = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Set OPENAI_API_KEY before running this example.");

        IReadOnlyDictionary<string, string> headers =
            new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" };
        return ValueTask.FromResult(headers);
    }
}
~~~

~~~sh
dotnet restore TheSingularityWorkshop.HeadlessAi.slnx
dotnet build TheSingularityWorkshop.HeadlessAi.slnx --configuration Release --no-restore
dotnet test TheSingularityWorkshop.HeadlessAi.slnx --configuration Release --no-build
~~~

Local benchmarks use fixture HTTP responses rather than live provider calls:

~~~sh
dotnet run --project benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks.csproj --configuration Release
~~~

Benchmark numbers should be reported only after the benchmark is actually run, with its environment and methodology recorded.

## Ecosystem fit and maturity

HeadlessAi can be used on its own by any .NET host that needs its HTTP invocation boundary. It does not require TheForge, FSM_COS, FSM_API, ProtocolAi, or GrammarAi. Compatible Workshop contracts can make integration easier, but adopting the whole ecosystem is not a prerequisite.

For concrete Workshop-specific examples—including AnyApp, browser experiences, semantic validation, FSM workflows, MicroBundles, Ontology, Renderer, Profiles, and Economy—see the [Ecosystem Integration guide](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/blob/development/docs/ecosystem-integration.md). That guide labels implemented capabilities separately from proposed integrations.

The current implementation is early development. Streaming, rich multimodal content, provider-specific tool semantics, global rate/cost budgets, and a host orchestration loop are not promised by this package. Direct HTTP does not bypass provider authentication, billing, rate limits, or access policies.

NuGet publication remains disabled in CI unless the repository owner explicitly approves a release.

---

<p align="center"><em>The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.</em><br><strong>Because state shouldn't be a mess.</strong><br><em>And because static boundaries are invitations to cause trouble.</em></p>
