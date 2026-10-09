# TheSingularityWorkshop.HeadlessAi

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml/badge.svg?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml)
[![Code Coverage](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi/branch/development/graph/badge.svg)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi)

<p align="center">
  <img src="docs/assets/headless-ai-architecture.svg" alt="HeadlessAi architecture and trust boundaries" width="100%">
</p>

**HeadlessAi is an HTTP-agent toolkit, not an AI model.** Configure endpoint-specific request and response behavior, then create lightweight agent instances that call those endpoints directly through ordinary HTTP.

Built-in adapters currently cover OpenAI Responses, Gemini generateContent, and Anthropic Messages. The raw, delegate, and JSON adapter seams let hosts call other authorized endpoints without a provider SDK. Endpoint schemas remain explicit; no universal request body is assumed.

## The problem it solves

Applications need to call one or many model endpoints without duplicating HTTP plumbing, coupling business logic to vendor SDKs, or assuming every provider accepts the same request format. HeadlessAi centralizes endpoint profiles, protocol adapters, and bounded invocation while leaving orchestration and authority with the host.

## First principles

- Provider-neutral transport, provider-specific protocols.
- Shared injected HttpClient; no socket pool per agent/request.
- Immutable profiles and reusable templates create many lightweight agent instances.
- Request-time credential headers, cancellation, timeouts, and bounded response bodies.
- ProtocolAi and GrammarAi are optional integrations, not dependencies.
- Model output is untrusted data, not permission to execute tools.
- Token savings, reduced drift, and accuracy gains must be measured rather than promised.

## Documentation

- [Documentation index](docs/README.md)
- [Problem domain](docs/problem-domain.md)
- [Theory](docs/THEORY.md)
- [Architecture boundaries](docs/architecture-boundaries.md)
- [Usage guide](docs/usage.md)
- [Provider adapters](docs/provider-adapters.md)
- [Security model](docs/security.md)
- [Performance](docs/performance.md)
- [Benchmark methodology](docs/benchmark-methodology.md)
- [Use cases](docs/use-cases.md)
- [Measurement plan](docs/measurement-plan.md)
- [Roadmap](docs/roadmap.md)
- [Repository map](docs/repository-map.md)
- [Changelog](CHANGELOG.md)

## Build

Requires the .NET 8 SDK.

    dotnet restore TheSingularityWorkshop.HeadlessAi.slnx
    dotnet build TheSingularityWorkshop.HeadlessAi.slnx --configuration Release --no-restore
    dotnet test TheSingularityWorkshop.HeadlessAi.slnx --configuration Release --no-build

Run local performance baselines:

    dotnet run --project benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks.csproj --configuration Release

## Scope and safety

HeadlessAi owns configured HTTP invocation and adapter contracts. It does not own a GUI, persistent conversation store, secret vault, workflow engine, FSM runtime, or authority to execute arbitrary model output. Direct HTTP does not bypass provider authentication, billing, rate limits, or access policies.

This is early development. NuGet publication is disabled in CI unless explicitly approved.
