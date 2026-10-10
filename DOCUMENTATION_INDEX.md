# HeadlessAi Documentation Index

This index follows the [Workshop Documentation Standard](DOCUMENTATION_STANDARD.md). The README is the public front door; focused documents provide deeper teaching and technical authority.

## Choose a path

| If you want to... | Start here |
|---|---|
| Understand the problem and purpose | [README](README.md), then [What is HeadlessAi?](docs/WHAT_IS_HEADLESSAI.md) |
| Understand domain vocabulary and non-goals | [Problem Domain](docs/problem-domain.md) |
| Learn why the boundary exists | [Theory](docs/THEORY.md) |
| Understand implementation ownership and lifecycle | [Architecture](docs/architecture-and-theory.md) |
| Understand dependency direction | [Architecture Boundaries](docs/architecture-boundaries.md) |
| Invoke an endpoint | [Usage Guide](docs/usage.md) |
| Configure a supported provider | [Provider Adapters](docs/provider-adapters.md) |
| Understand tested provider parsing failures | [Provider Failure Contract](docs/provider-failure-contract.md) |
| Handle credentials and untrusted responses | [Security Model](docs/security.md) |
| See concrete uses across Workshop repositories | [Ecosystem Integration](docs/ecosystem-integration.md) |
| Understand benchmark limits | [Benchmark Methodology](docs/benchmark-methodology.md) and [Performance](docs/performance.md) |
| Evaluate ProtocolAi + GrammarAi | [Measurement Plan](docs/measurement-plan.md) |
| Understand practical fit and non-fit | [Use Cases](docs/use-cases.md) |
| Evaluate an alpha candidate and release gates | [Alpha Readiness](docs/ALPHA_READINESS.md) |
| Build, test, and plan work | [Contributing](CONTRIBUTING.md), [Roadmap](docs/roadmap.md), and [Continuation Brief](docs/CONTINUATION_BRIEF.md) |

## Document responsibilities

- README: identity, reader journey, first-minute proof, boundaries, and navigation.
- What-is guide: accessible conceptual explanation.
- Theory: rationale, mental model, assumptions, and invariants.
- Problem domain: vocabulary, scope, lifecycle, and acceptance criteria.
- Architecture guides: implementation shape and responsibility ownership.
- Usage/provider guides: practical use and provider-specific settings.
- Ecosystem integration: concrete cross-repository scenarios, current evidence versus opportunities, dependency direction, and host-owned safety rules.
- Security: credential and trust model.
- Performance and measurement guides: evidence, limitations, and open hypotheses.
- Alpha readiness: concrete pre-release gates, evidence to retain, and publication safety.
- Roadmap, contribution guide, and continuation brief: ongoing development and release posture.

Source and tests determine current behavior. If documentation and implementation disagree, record and resolve the discrepancy rather than assuming the prose is correct. Distinguish current source, published artifacts, design intent, and future work.
