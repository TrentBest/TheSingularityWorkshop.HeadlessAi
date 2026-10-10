# Contributing

- Keep master as reviewed truth and development as the integration branch.
- Use short-lived exploratory branches for focused work.
- Keep dependencies minimal and architecture boundaries explicit.
- Add deterministic tests for success and failure paths; tests must not need live endpoints or secrets.
- Update docs when public contracts, security assumptions, or architecture decisions change.
- Verify restore, Release build, tests, coverage, and package contents before a release.
- Never publish NuGet without explicit approval; workflow publication is disabled by default.
- Do not claim token, cost, hallucination, drift, or performance improvements without reproducible evidence.
