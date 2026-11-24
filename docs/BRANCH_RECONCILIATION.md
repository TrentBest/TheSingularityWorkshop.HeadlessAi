<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Branch Reconciliation Ledger

**Review date:** 2026-11-24 UTC  
**Scope:** all persistent HeadlessAi branches, compared with `development`  
**Disposition:** Inventory and semantic reconciliation only. No merge, cherry-pick, reset, force-push, or branch deletion has been performed. NuGet publication remains unauthorized.

## Live audit — 2026-11-24 UTC

GitHub reports **9 persistent branches**. The required permanent inventory is exactly `master` and `development`. Comparisons below are against the current `development` head (11 commits behind the active integration branches at this checkpoint).

### Verified inventory

| Branch | Commits ahead / behind `development` | Evidence and disposition |
|---|---:|---|
| `master` | 32 / 161 | **PRESERVE** — released baseline. Its history and release identity must be reconciled deliberately; do not reset or delete it. |
| `development` | canonical | **PRESERVE** — active integration baseline. |
| `docs/headlessai-ecosystem-adoption-assessment` | 3 / 11 | **PRESERVE** — open draft PR #16; one changed file. Review the assessment against current repository reality before integration. |
| `docs/readme-contrast-accessibility` | 1 / 104 | **PRESERVE / REVIEW** — one unique documentation/visual commit remains. Similar contrast treatment elsewhere is not proof that its specific content is superseded. |
| `feature/bounded-artifact-http-adapter` | 0 / 14 | **HOLD** — no unique commits or file diff remain against current `development`, but Issue #12 is still open. Its requirement explicitly calls for the optional HeadlessAi integration to delegate to FSM_REST's shared bounded transport and connection-time remote-IP policy. Execution PR #50 contains a separate pinned HTTPS artifact transport, but that does not satisfy the shared FSM_REST ownership requirement. Do not mark ready for deletion until Issue #12 and the FSM_REST transport ownership work are reconciled. |
| `feature/headlessai-ci-workflow-validation` | 205 / 11 | **PRESERVE / RECONCILE** — 51 changed files. It overlaps the later fix branch in scope; compare the unique commit and file-level semantics before deciding whether either branch is superseded. |
| `fix/headlessai-ci-workflow-validation` | 202 / 11 | **PRESERVE / RECONCILE** — 51 changed files. Similarity to the feature branch is not enough to delete either history; reconcile the exact workflow fixes and their tests first. |
| `feature/resumable-agent-turn-integration` | 218 / 11 | **PRESERVE** — open draft PR #13; current exact head `309e75aa1647a51a10516d7d4abd2556efc48a35`. This is the active resumable-turn integration candidate. |
| `work/http-retry-after-guidance` | 15 / 104 | **PRESERVE / REVIEW** — 15 unique commits and 7 changed paths remain. Retry-after behavior may overlap PR #13, but the branch's unique history has not been fully reconciled semantically. |

### Active pull requests and exact-head evidence

- PR #13: [Integrate v1 resumable agent-turn contracts](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/pull/13), from `feature/resumable-agent-turn-integration` into `development`; draft, open, and unmerged.
- PR #16: [Document HeadlessAi adoption assessment across Workshop repositories](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/pull/16), from `docs/headlessai-ecosystem-adoption-assessment` into `development`; draft, open, and unmerged.
- Exact-head Development Build, Test, and Coverage [run #45340341966](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/45340341966) passed on `309e75aa1647a51a10516d7d4abd2556efc48a35`: build succeeded; **263 tests passed, 0 failed, 0 skipped**; coverage was prepared and Codecov upload completed.
- The same exact-head run's **Non-publishing exact-candidate package verification** job passed: restore/build/test against published FSM_Serialization 1.0.0 succeeded, the `0.1.0-alpha.2` package archive audit passed, clean-consumer restore/build passed, and a review artifact was uploaded. No package was published.
- Sith Policy remains red because the repository has **9 branches**, not exactly `master` and `development`.

### Owner cleanup batch

**No surplus branch is currently marked READY FOR OWNER DELETION.** In particular:

- The bounded-artifact branch has no remaining diff but is still referenced by open Issue #12, whose transport ownership requirements are not yet met.
- The two workflow-validation branches have substantial unique history and need semantic reconciliation.
- The documentation and retry-after branches retain unique content.
- PR #13 and PR #16 remain open and draft.

The owner performs any eventual deletion. Do not delete branches automatically. Group only branches with completed semantic/reference reconciliation into one owner-reviewed cleanup batch.

### Release and authority boundary

- The ordinary development workflow builds/tests the source and reports coverage.
- The non-publishing candidate verification workflow validates the packed artifact and a clean consumer. Its uploaded package artifact is for review only.
- NuGet publication remains disabled until Trent explicitly authorizes a specific release.
- No merge, release, deployment, canonical-branch write, privileged workflow dispatch, or branch deletion has been performed.

---

## Historical branch evidence — 2026-10-22

- Previous verified implementation head: `eac7f44562a4074bb3eac0d30df35861ecb6114c`.
- That head's CI run [#39717094485](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/39717094485) passed.
- Follow-up documentation commits added `docs/DOMAIN_COMPLETENESS.md`, indexed it in `DOCUMENTATION_INDEX.md`, and refreshed `docs/ALPHA_READINESS.md`; latest known documentation commit before this ledger refresh: `e4a374efca25c6a046e7e2abbc6926a326f8f008`.
- `master` snapshot at last branch comparison: `d4aabc04c1f6e34c926a37b5340b62de0fc5c3d9`; previous comparison was 18 commits ahead and 8 behind. Refresh the comparison before any branch disposition.
- No open pull requests were reported during the previous review; recheck before any reconciliation decision.
- CI for the follow-up documentation commits is not yet verified. This ledger refresh is also documentation-only and must pass CI before the new head is treated as verified.

## Work found only on master

| Commit | Change | Reconciliation treatment |
|---|---|---|
| `fea455c173bd` | Build HeadlessAi package foundation and architecture | Historical foundation commit; inspect its tree/history if any unique implementation remains outside development. |
| `3f80214b4b67` | Disable NuGet publishing after failed first-release attempt | Preserve the safety intent. |
| `a2dc245daf55` | Use NuGet Trusted Publishing for gated release job | Preserve the trusted-publishing approach only with the safer immutable action reference now present in development. |
| `c66d277f17b7` | Temporarily enable approved alpha.1 publication | Historical release event, not current authorization. Do not repeat or interpret as approval for future publication. |
| `638af2b59b14` | Restore disabled publication gate | Preserve the safety intent. |
| `25f6a7c1c883` | Enable automatic verified publication on master | Historical and superseded by the following safety-restoration commit. Do not revive this behavior. |
| `ac9c347962e8` | Restore safe default: disable automatic publication | Preserve the safety intent; current development uses an unconditional `if: ${{ false }}` publication gate. |
| `d4aabc04c1f6` | Add shared Workshop moniker to README | Preserve the moniker and accessible image alternative in the final README. |

The master README is 163 lines and development's README is 160 lines. The master README points at the canonical documentation standard in the old `development` location; development now points to the standard's current location on FSM_COS `master`.

## Release-critical differences

- `master` retains the previously published `0.1.0-alpha.1` baseline and its .NET 8 target. The active `development` line and PR #13 candidate advance to `0.1.0-alpha.2` and target .NET 10.
- Development includes Markdown under `docs/` and SVG assets in the NuGet package. Master packs only the root README in the project file. Keep the development package-documentation behavior.
- The current development workflow has no package-publishing job and no pack step. The manual release-verification workflow has read-only repository permissions and no publishing step.
- Development pins NuGet Trusted Publishing's action to an immutable commit SHA; master uses a mutable version tag.
- The ordinary development workflow does not produce a package artifact or run the package archive audit. Those checks belong to the separate manual release-verification workflow and remain unverified until that workflow is dispatched successfully.

## Required before declaring branch reconciliation complete

- [ ] Compare the master-only README additions with the development README and preserve useful identity/standard content.
- [x] Refresh exact PR head and ahead/behind count after the release-candidate documentation and version changes; current exact-head CI is in progress.
- [ ] Use [Domain Completeness Plan](DOMAIN_COMPLETENESS.md) as the ordered source of truth for contract, security, docs, and artifact gates.
- [ ] Verify all provider adapters and their failure/security contracts against source and tests.
- [ ] Review the full workflow history; never restore automatic publication.
- [ ] Confirm the package's docs and SVG assets remain in the artifact.
- [ ] Re-run CI on the exact reconciled candidate.
- [ ] Obtain repository-owner approval before merging or deleting any branch.

**This ledger is not authorization to delete `master` or any other branch.**

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
