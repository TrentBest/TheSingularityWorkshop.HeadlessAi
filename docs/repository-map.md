<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Repository map

- README.md: purpose, build instructions, and entry points.
- TheSingularityWorkshop.HeadlessAi.slnx: solution definition.
- src/TheSingularityWorkshop.HeadlessAi/: endpoint profile, normalized data contracts, agent/template, request-time header provider, and raw/delegate/JSON plus OpenAI/Gemini/Anthropic adapters.
- tests/TheSingularityWorkshop.HeadlessAi.Tests/: deterministic in-memory HTTP tests for profiles, transport, adapters, timeouts, cancellation, and response bounds.
- benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks/: local BenchmarkDotNet baselines without live network calls.
- docs/: theory, usage, security, adapters, performance, use cases, measurement plan, roadmap, and this map.
- .github/workflows/build.yml: restore, build, test, coverage/Codecov, pack, artifact; publishing disabled.

The library owns endpoint invocation and protocol extension points only. Tests and benchmarks do not require live endpoints or secrets. Documentation distinguishes implemented behavior from planned work.

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
