# HeadlessAi Documentation Standard

HeadlessAi adopts the shared Workshop documentation standard defined in [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md). That repository is the canonical source; this file records the local application and rules specific to this repository.

The goal is **edify, not mystify**: explain the human problem and mental model, state technical boundaries precisely, provide an honest first success, and support claims with evidence.

## Shared README journey

Where relevant, use this opening sequence:

| ID | Marker | Purpose |
|---|---|---|
| 00 | ✳️ | Project identity and valid badges |
| 01 | 🟦 | Problem and short response |
| 02 | 🟣 | Documentation map and ecosystem context |
| 03 | 🩵 | Deeper explanation and responsibility boundary |
| 04 | 🟢 | First-minute proof with prerequisites and expected behavior |
| 05 | 🟪 | Documentation and theory links with purpose statements |

Place the package-specific money-shot visual below the title/badges and before section 01. These identifiers are for the canonical front-door topics, not a numbering scheme for every heading. GitHub Markdown does not reliably support arbitrary heading colors, so use the number, a recognizable marker, and explicit heading text together. Never rely on color alone.

## Document responsibilities

- README: orientation, first proof, boundaries, and links—not the whole manual.
- What-is guide: accessible mental model for new and non-coder readers.
- Theory: rationale, assumptions, trade-offs, and invariants.
- Problem domain: vocabulary, scope, non-goals, lifecycle, and acceptance criteria.
- Architecture: component relationships, ownership, dependency direction, and handoff.
- Usage/provider guides: examples, settings, expected behavior, and limitations.
- Security: credentials, untrusted output, transport limits, and host responsibility.
- Performance/measurement: separate measured facts, derived observations, hypotheses, and planned experiments.
- Documentation index: reader tasks and authoritative documents.
- Development/continuation guides: build/test evidence, current status, and next steps.

## Evidence and honesty

1. Source and tests are authoritative for current implementation behavior.
2. Label examples as verified, source-checked, or conceptual; do not imply a live request is an offline example.
3. Distinguish current source from behavior in a published NuGet package.
4. Do not invent test results, performance, token savings, quality improvements, or compatibility guarantees.
5. Describe why this package exists separately from adjacent packages and what it deliberately does not own.
6. Explain why a dependency is used; link to a neighbor's authoritative domain documentation rather than copying it.
7. Use architecture diagrams for real responsibilities and flows, with meaningful alt text and a caption.
8. Check relative links, code, commands, badges, project names, and maturity statements against the target branch.
9. Update docs when contracts, security assumptions, or architecture decisions change.
10. Never imply NuGet publication, release, merge, or deployment without evidence and explicit authorization.

## Visual and editorial craft

Use vivid semantic markers consistently, but pair color with numbers and words. Prefer local SVGs for architecture diagrams; keep them readable in GitHub and on mobile. Explain abstract concepts through concrete problems and analogies, then state where an analogy stops. Avoid badge walls, decorative clutter, hype, and claims without evidence.

Use the Workshop footer where appropriate:

> The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.  
> **Because state shouldn't be a mess.**  
> And because static boundaries are invitations to cause trouble.

## Review checklist

- [ ] README follows the six-part opening journey where applicable.
- [ ] The money-shot visual depicts actual package responsibilities.
- [ ] A new reader can find the problem, first success, and deeper path quickly.
- [ ] The package's own domain and non-goals are explicit.
- [ ] Dependency direction and host ownership are accurate.
- [ ] Examples and commands match source and clearly state whether they were run.
- [ ] Performance and AI-quality claims are evidence-backed or labeled hypotheses.
- [ ] Links, badges, alt text, and footer are correct.
- [ ] Current source, published package, and future work are distinguished.
