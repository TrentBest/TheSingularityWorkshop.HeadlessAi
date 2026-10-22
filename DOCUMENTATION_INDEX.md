<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# HeadlessAi Documentation Index

This index follows the [Workshop Documentation Standard](DOCUMENTATION_STANDARD.md). The README is the public front door; focused documents provide deeper teaching and technical authority.

## Choose a path

| If you want to... | Start here |
|---|---|
| Understand the problem and purpose | [README](README.md), then [What is HeadlessAi?](docs/WHAT_IS_HEADLESSAI.md) |
| Understand domain vocabulary and non-goals | [Problem Domain](docs/problem-domain.md) |
| Track domain completeness and release attack plan | [Domain Completeness Plan](docs/DOMAIN_COMPLETENESS.md) |
| Learn why the boundary exists | [Theory](docs/THEORY.md) |
| Understand implementation ownership and lifecycle | [Architecture](docs/architecture-and-theory.md) |
| Understand dependency direction | [Architecture Boundaries](docs/architecture-boundaries.md) |
| Invoke an endpoint using the current public API | [Usage Guide](docs/usage.md) |
| Understand the proposed fluent configuration direction | [Fluent Configuration Design](docs/fluent-configuration-design.md) |
| Configure a supported provider | [Provider Adapters](docs/provider-adapters.md) |
| Understand tested provider parsing failures | [Provider Failure Contract](docs/provider-failure-contract.md) |
| Handle credentials and untrusted responses | [Security Model](docs/security.md) |
| See concrete uses across Workshop repositories | [Ecosystem Integration](docs/ecosystem-integration.md) |
| Understand benchmark limits | [Benchmark Methodology](docs/benchmark-methodology.md) and [Performance](docs/performance.md) |
| Evaluate ProtocolAi + GrammarAi | [Measurement Plan](docs/measurement-plan.md) |
| Understand practical fit and non-fit | [Use Cases](docs/use-cases.md) |
| Evaluate an alpha candidate and release gates | [Alpha Readiness](docs/ALPHA_READINESS.md) |
| Reconcile branch history without losing unique work | [Branch Reconciliation Ledger](docs/BRANCH_RECONCILIATION.md) |
| Browse the architecture visual and evidence links | [Gallery](docs/GALLERY.md) |
| Build, test, and plan work | [Contributing](CONTRIBUTING.md), [Roadmap](docs/roadmap.md), and [Continuation Brief](docs/CONTINUATION_BRIEF.md) |

## Document responsibilities

- README: identity, reader journey, first-minute proof, boundaries, and navigation.
- What-is guide: accessible conceptual explanation.
- Theory: rationale, mental model, assumptions, and invariants.
- Problem domain: vocabulary, scope, lifecycle, and acceptance criteria.
- Architecture guides: implementation shape and responsibility ownership.
- Usage/provider guides: practical use and provider-specific settings.
- Fluent configuration design: proposed ergonomics, provider-neutral constraints, and acceptance criteria; not a shipped API contract.
- Ecosystem integration: concrete cross-repository scenarios, current evidence versus opportunities, dependency direction, and host-owned safety rules.
- Security: credential and trust model.
- Performance and measurement guides: evidence, limitations, and open hypotheses.
- Alpha readiness: concrete pre-release gates, evidence to retain, and publication safety.
- Roadmap, contribution guide, and continuation brief: ongoing development and release posture.

Source and tests determine current behavior. If documentation and implementation disagree, record and resolve the discrepancy rather than assuming the prose is correct. Distinguish current source, published artifacts, design intent, and future work.

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
