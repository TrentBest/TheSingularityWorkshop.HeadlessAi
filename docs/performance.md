# Performance and benchmarking

The desired hot path is: create one endpoint-specific request, send through a reused transport, buffer only a bounded response, extract the response once, and return normalized data. Avoid mandatory provider SDKs, reflection dispatch, hidden per-request caches, and unnecessary JSON conversions.

Remote inference usually dominates local allocations, but a Forge can create many agent instances and issue concurrent calls. Agent instances should stay lightweight and transport reuse must be explicit. Profile dictionaries are copied and exposed read-only at construction; dynamic credentials are resolved only when an invocation needs them.

Benchmark profile/agent construction, fake-handler round trips, adapter request creation, response extraction, many agents sharing one transport, and bounded concurrency. Report runtime, OS, CPU, allocations, throughput, and configuration. Keep live-provider tests opt-in and never require CI secrets. Do not publish numerical performance or token-efficiency claims until results are reproducible.
