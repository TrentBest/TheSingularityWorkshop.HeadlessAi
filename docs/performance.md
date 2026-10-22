<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Performance and benchmarking

The desired hot path is: create one endpoint-specific request, send through a reused transport, buffer only a bounded response, extract the response once, and return normalized data. Avoid mandatory provider SDKs, reflection dispatch, hidden per-request caches, and unnecessary JSON conversions.

Remote inference usually dominates local allocations, but a Forge can create many agent instances and issue concurrent calls. Agent instances should stay lightweight and transport reuse must be explicit. Profile dictionaries are copied and exposed read-only at construction; dynamic credentials are resolved only when an invocation needs them.

## Run the local benchmarks

The BenchmarkDotNet project measures agent construction and a fake-handler round trip without a live endpoint or secret:

    dotnet run --project benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks.csproj --configuration Release

The current benchmark suite is a starting baseline, not a performance claim. Run it on a documented machine/runtime and retain the generated report when comparing changes. Future scenarios should include profile construction, adapter request creation, response extraction, many agents sharing one transport, and bounded concurrency.

Report runtime, OS, CPU, allocations, throughput, and configuration. Keep live-provider tests opt-in and never require CI secrets. Do not publish numerical performance or token-efficiency claims until results are reproducible.

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
