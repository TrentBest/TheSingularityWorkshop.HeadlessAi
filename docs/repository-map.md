# Repository map

- README.md: purpose, build instructions, and entry points.
- TheSingularityWorkshop.HeadlessAi.slnx: solution definition.
- src/TheSingularityWorkshop.HeadlessAi/: endpoint profile, normalized data contracts, agent/template, request-time header provider, and raw/delegate/JSON plus OpenAI/Gemini/Anthropic adapters.
- tests/TheSingularityWorkshop.HeadlessAi.Tests/: deterministic in-memory HTTP tests for profiles, transport, adapters, timeouts, cancellation, and response bounds.
- benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks/: local BenchmarkDotNet baselines without live network calls.
- docs/: theory, usage, security, adapters, performance, use cases, measurement plan, roadmap, and this map.
- .github/workflows/build.yml: restore, build, test, coverage/Codecov, pack, artifact; publishing disabled.

The library owns endpoint invocation and protocol extension points only. Tests and benchmarks do not require live endpoints or secrets. Documentation distinguishes implemented behavior from planned work.
