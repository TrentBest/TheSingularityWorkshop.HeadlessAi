# Repository map

- README.md: purpose, build instructions, and entry points.
- TheSingularityWorkshop.HeadlessAi.slnx: solution definition.
- src/TheSingularityWorkshop.HeadlessAi/: transport primitives and adapter seam.
- tests/TheSingularityWorkshop.HeadlessAi.Tests/: deterministic tests using in-memory HTTP handlers.
- docs/: theory, usage, security, provider adapters, performance, use cases, measurements, roadmap, and this map.
- .github/workflows/build.yml: restore, build, test, coverage, pack, artifact; publishing disabled.

The library owns endpoint invocation and protocol extension points only. Tests do not require live endpoints or secrets. The documentation distinguishes implemented behavior from roadmap items.
