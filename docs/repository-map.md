# Repository map

- README.md: purpose, build instructions, and entry points.
- TheSingularityWorkshop.HeadlessAi.slnx: solution definition.
- src/TheSingularityWorkshop.HeadlessAi/: endpoint profile, normalized data contracts, agent, and adapter seams.
- tests/TheSingularityWorkshop.HeadlessAi.Tests/: deterministic in-memory HTTP tests.
- docs/: theory, usage, security, adapters, performance, use cases, measurement plan, roadmap, and this map.
- .github/workflows/build.yml: restore, build, test, coverage, pack, artifact; publishing disabled.

The library owns endpoint invocation and protocol extension points only. Tests do not require live endpoints or secrets. Documentation distinguishes implemented behavior from planned work.
