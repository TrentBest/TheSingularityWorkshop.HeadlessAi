# Benchmark Methodology

This document separates HeadlessAi's local overhead from remote provider performance and model-output quality.

## What a local benchmark can establish

A deterministic local benchmark can measure profile/template/agent construction, request construction and JSON serialization, response parsing and metadata extraction, managed allocations for fixture responses, and overhead across repeated logical callers.

It cannot establish provider inference latency, real-world network reliability, ProtocolAi/GrammarAi token savings, model accuracy or hallucination rates, or production cost without current provider pricing and usage data.

## Benchmark rules

1. Do not call live endpoints in ordinary microbenchmarks.
2. Use fixed request inputs and fixture response bodies.
3. Reuse the same injected HttpClient and fake HttpMessageHandler for equivalent cases.
4. Separate cold construction from steady-state invocation.
5. Report runtime, target framework, OS, CPU, benchmark commit, configuration, iteration settings, and allocations.
6. Compare equivalent response sizes and adapter work.
7. Avoid conclusions from a single run; use BenchmarkDotNet statistics and retain raw output.
8. Treat network and model latency as a separate end-to-end experiment.

## Suggested benchmark matrix

| Case | Question |
|---|---|
| Profile and template construction | Is configuration setup lightweight? |
| Agent creation | What is the marginal cost of one logical caller? |
| JSON request creation | What are allocations and serialization costs? |
| Small/medium/large fixture response | How does parsing scale with payload size? |
| Usage metadata extraction | What extra work does normalization add? |
| 1, 16, 64, 256 logical agents, sequential | Does template reuse change per-agent overhead? |
| Bounded parallel invocation through fake handler | What local allocations and queueing costs occur under concurrency? |

## Token and model-quality experiments

Token experiments are not microbenchmarks. Use a versioned task set and compare a natural-language baseline against protocol/grammar conditions. Count setup, cache, output, repair, and repeated-call costs. Keep model, version, task, and generation settings comparable.

Measure task success, output validity, semantic drift, independently reviewed unsupported claims, latency, and correction time. Report sample sizes, uncertainty, exclusions, and failure modes. See [Measurement plan](measurement-plan.md).

## Release rule

Performance claims in README, package metadata, and articles must point to reproducible results. Until then, say “designed to avoid redundant transport overhead” rather than claiming the package is faster or more energy efficient.
