# TheSingularityWorkshop.HeadlessAi

[![Build](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml/badge.svg?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/workflows/build.yml)
[![codecov](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi/branch/development/graph/badge.svg)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.HeadlessAi)

**HeadlessAi is an HTTP-agent toolkit, not an AI model.** Configure endpoint-specific request and response behavior, then create lightweight agent instances that call those endpoints directly through ordinary HTTP.

Provider request formats are not assumed to be universal. HeadlessAi provides a small transport core and explicit adapter seams; the host owns agent roles, permissions, budgets, memory, and task orchestration.

## First principles

- Provider-neutral transport, provider-specific protocols.
- Shared injected HttpClient; no socket pool per agent/request.
- ProtocolAi and GrammarAi are optional integrations, not dependencies.
- Model output is untrusted data, not permission to execute tools.
- Token savings, reduced drift, and accuracy gains must be measured rather than promised.

## Documentation

- [Documentation index](docs/README.md)
- [Architecture and theory](docs/architecture-and-theory.md)
- [Usage guide](docs/usage.md)
- [Provider adapters](docs/provider-adapters.md)
- [Security model](docs/security.md)
- [Performance](docs/performance.md)
- [Use cases](docs/use-cases.md)
- [Measurement plan](docs/measurement-plan.md)
- [Roadmap](docs/roadmap.md)
- [Repository map](docs/repository-map.md)

## Build

Requires the .NET 8 SDK.

    dotnet restore TheSingularityWorkshop.HeadlessAi.slnx
    dotnet build TheSingularityWorkshop.HeadlessAi.slnx --configuration Release --no-restore
    dotnet test TheSingularityWorkshop.HeadlessAi.slnx --configuration Release --no-build

## Scope

HeadlessAi owns configured HTTP invocation and adapter contracts. It does not own a GUI, persistent conversation store, secret vault, workflow engine, FSM runtime, or authority to execute arbitrary model output. Early development; no NuGet publication is enabled by default.
