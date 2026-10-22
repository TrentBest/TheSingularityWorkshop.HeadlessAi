<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# HeadlessAi Gallery

HeadlessAi provides provider-neutral HTTP invocation primitives with explicit endpoint-specific adapters. This gallery separates the explanatory architecture picture from deterministic fixtures that verify request mapping and failure behavior.

## Visual tour

### The endpoint invocation boundary

![HeadlessAi architecture and trust boundaries](assets/headless-ai-architecture.svg)

This authored architecture diagram shows the responsibility split: the host owns intent, permissions, memory, and validation; HeadlessAi maps normalized input to an endpoint protocol and bounds the HTTP response; the provider owns its API and inference. ProtocolAi and GrammarAi are optional host-side helpers, not required dependencies of HeadlessAi's core.

The picture is explanatory artwork—not a live request trace, generated UML, or proof that a remote provider was contacted.

### README journey markers

The SVGs in [section-dividers](assets/section-dividers/) are navigation artwork for the six-part README journey, not implementation diagrams.

## See actual behavior

From the repository root:

```bash
dotnet test tests/TheSingularityWorkshop.HeadlessAi.Tests/TheSingularityWorkshop.HeadlessAi.Tests.csproj --configuration Release
dotnet pack src/TheSingularityWorkshop.HeadlessAi/TheSingularityWorkshop.HeadlessAi.csproj --configuration Release
```

The tests use deterministic HTTP fixtures; they do not require paid live-provider calls. They cover adapter request/response behavior, malformed and empty responses, timeout and response-size boundaries, and invalid settings. A successful fixture test is not proof of compatibility with every provider API variant.

## Detailed recipe

Start with [Usage](usage.md), then read [Provider Adapters](provider-adapters.md), [Provider Failure Contract](provider-failure-contract.md), [Security Model](security.md), [Architecture Boundaries](architecture-boundaries.md), and [Alpha Readiness](ALPHA_READINESS.md).

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
