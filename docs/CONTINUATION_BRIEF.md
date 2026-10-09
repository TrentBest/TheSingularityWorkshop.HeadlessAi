# Continuation Brief — HeadlessAi

## Purpose

HeadlessAi is a standalone .NET HTTP-agent toolkit. It configures endpoint-specific HTTP requests and responses and lets hosts create lightweight agent instances from reusable profiles/templates. It is not itself a model, workflow engine, permission system, memory store, or execution loop.

## Architecture invariants

- Keep endpoint-specific schemas explicit; do not force all providers into one JSON body.
- Share an injected HttpClient; agent instances must not own independent connection pools.
- Keep ProtocolAi and GrammarAi optional. HeadlessAi must work without them and must not depend on TheForge, FSM_COS, or FSM_API.
- The host owns roles, memory, scheduling, permissions, budgets, validation, tool mediation, and consequential actions.
- Treat model output as untrusted data, never as permission to execute.
- Use request-time header providers for rotating secrets; avoid secret-bearing diagnostics.
- Bound response bodies, timeouts, and cancellation behavior.
- Never claim token savings, accuracy, hallucination reduction, or performance improvement without reproducible measurements.
- Never publish to NuGet without explicit user approval. CI publication must remain disabled by default.

## Current repository structure

- src/TheSingularityWorkshop.HeadlessAi/: library contracts and provider adapters.
- tests/TheSingularityWorkshop.HeadlessAi.Tests/: deterministic fixture-based tests.
- benchmarks/TheSingularityWorkshop.HeadlessAi.Benchmarks/: local BenchmarkDotNet baselines.
- docs/: problem domain, theory, architecture, usage, provider adapter, security, performance, use cases, measurement, and roadmap documentation.
- .github/workflows/build.yml: restore, Release build, tests, coverage/Codecov, package pack, and artifact upload; no publish step enabled.

## Current implementation slice

- Endpoint profiles and reusable agent templates.
- Raw text, delegate-based, and JSON adapter extension points.
- Direct HTTP adapters for OpenAI Responses, Gemini generateContent, and Anthropic Messages.
- Request-time header resolution, cancellation, timeouts, and bounded response bodies.
- Usage metadata extraction for supported response fields.
- Fixture-based provider request/response tests.
- BenchmarkDotNet project and CI coverage/pack workflow.

The built-in adapters currently focus on basic text requests. Streaming, rich multimodal inputs, full provider tool semantics, broad error-envelope normalization, explicit redirect policy, retry policy, and host-level rate/cost budgets remain future work or host responsibilities.

## Current branch and review posture

Work is on development. PR #1 targets master and remains unmerged. Do not merge, publish, or rewrite protected branch history without explicit user direction. After every meaningful batch, verify CI against the exact branch head and report what did and did not run.

## Next logical work

1. Run CI on the exact latest head and fix all warnings/errors.
2. Audit provider adapters against current official endpoint schemas and add malformed/error-envelope fixtures.
3. Audit package metadata, XML docs, license, changelog, repository links, and packed package contents.
4. Expand benchmark cases and capture a baseline; do not convert intended performance into claims.
5. Design optional ProtocolAi/GrammarAi evaluation harness and collect reproducible token/quality results.
6. Decide and document retry, redirect, concurrency, and budget boundaries before implementation.
7. Keep NuGet publication disabled until the user explicitly approves a release.
