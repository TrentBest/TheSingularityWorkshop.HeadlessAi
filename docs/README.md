# Documentation index

Start here if you are new to the package.

- [Problem Domain](problem-domain.md) — the problem, domain vocabulary, lifecycle, non-goals, and acceptance criteria.
- [Theory](THEORY.md) — why the package boundary and design invariants exist.
- [Architecture and Theory](architecture-and-theory.md) — core contracts, profile/template/agent lifecycle, and invariants.
- [Architecture Boundaries](architecture-boundaries.md) — dependency direction, ownership matrix, and failure semantics.
- [Usage Guide](usage.md) — create profiles, templates, and agents.
- [Provider Adapters](provider-adapters.md) — endpoint-specific protocol mappings and limitations.
- [Security Model](security.md) — credentials, response limits, and trust boundaries.
- [Performance](performance.md) — allocation and latency principles.
- [Benchmark Methodology](benchmark-methodology.md) — what local benchmarks can and cannot prove.
- [Use Cases](use-cases.md) — Forge orchestration and independent application scenarios.
- [Measurement Plan](measurement-plan.md) — test token reduction, output validity, semantic drift, and unsupported claims.
- [Roadmap](roadmap.md) — staged implementation and release gates.
- [Repository Map](repository-map.md) — where responsibilities and files live.
- [Changelog](../CHANGELOG.md) — recorded repository changes.

## Visual architecture

![HeadlessAi architecture and trust boundaries](assets/headless-ai-architecture.svg)

The host owns intent, permissions, memory, validation, and consequential actions. HeadlessAi owns endpoint invocation and protocol adapters. Provider endpoints retain their own schemas and access policies.
