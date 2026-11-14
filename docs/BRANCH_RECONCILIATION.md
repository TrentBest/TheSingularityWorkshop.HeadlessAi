<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Branch Reconciliation Ledger

**Review date:** 2026-11-12 UTC  
**Scope:** all persistent branches, with comparisons against `development`  
**Disposition:** Audit and documentation reconciliation only. No merge, cherry-pick, reset, force-push, or branch deletion has been performed. NuGet publication remains unauthorized.

## Live audit — 2026-11-12 UTC

GitHub reports **7 persistent branches**. The comparisons below were refreshed against the current `development` head `3cb5ace24fe97822eaa1e9b65b3aa8da8589ab07`.

### Verified inventory

| Branch | Commits ahead / behind `development` | Evidence and disposition |
|---|---:|---|
| `master` | 19 / 150 | PRESERVE — released baseline. Its divergence includes workflow and Sith-policy differences; no blind reconciliation. |
| `development` | canonical | PRESERVE — active integration baseline. |
| `feature/resumable-agent-turn-integration` | 126 / 0 | PRESERVE — active PR #13 head `7f3df570f58686481e0ab797cae9de5094ad8188`. Latest completed source CI [run #43161297815](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/43161297815) passed 248 tests at `bb10aaf02d98191c5a4ba0064d1f54885bdcf9e7`; later commits are documentation and release-workflow integration changes. |
| `feature/resumable-agent-turn-contracts` | 159 / 72 | PRESERVE — 159 unique commits and substantial divergence. Do not discard its history merely because PR #11 was superseded. |
| `work/http-retry-after-guidance` | 15 / 93 | PRESERVE — 15 unique commits and seven changed paths remain. PR #13 contains related retry-after behavior, but this branch's unique history has not been fully reconciled semantically. |
| `docs/readme-contrast-accessibility` | 1 / 93 | PRESERVE — one unique architecture-SVG commit remains. Similar contrast treatment in PR #13 is not by itself proof that the original change is fully superseded. |
| `feature/bounded-artifact-http-adapter` | 0 / 3 | HOLD — no unique commits or file diff remain against `development`, but HeadlessAi Issue #12 is still open and specifies the bounded artifact-fetch integration needed by Foundry. Do not mark ready for deletion until Issue #12, Foundry #8, and the FSM_REST #13/#15 transport ownership work are reconciled and any useful branch-specific design is preserved. |

### Owner cleanup batch

**No surplus branch is currently marked READY FOR OWNER DELETION by this audit.** In particular, the zero-ahead bounded-adapter branch is only a candidate: its associated Issue #12 remains open, so commit-count equality does not establish that the work or design intent is disposable.

The owner performs any eventual deletion. Do not delete branches automatically, and group only branches with completed semantic/reference reconciliation into one owner-reviewed cleanup batch.

### Active integration and exact-head verification

- PR #13: [Integrate v1 resumable agent-turn contracts](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/pull/13), from `feature/resumable-agent-turn-integration` into `development`; it remains draft, open, and unmerged.
- Current PR head at this ledger update: `7f3df570f58686481e0ab797cae9de5094ad8188` (126 commits ahead, 0 behind `development`).
- Prior exact-head Development Build, Test, and Coverage run [#43160452720](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/43160452720) completed successfully: build succeeded; **248 tests passed, 0 failed, 0 skipped**; coverage preparation and Codecov upload succeeded. Follow-up run [#43161297815](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/43161297815) also completed successfully: build succeeded; **248 tests passed, 0 failed, 0 skipped**; coverage preparation and Codecov upload succeeded. Its tested SHA is `bb10aaf02d98191c5a4ba0064d1f54885bdcf9e7`; later changes are documentation/workflow integration only.
- The permanent branch-policy check remains failing because seven branches exist instead of exactly `master` and `development`. This policy failure is not a reason to delete unreconciled work.

### Release and authority boundary

- Issue #15 tracks the non-publishing NuGet verification gate. It now exposes manual dispatch and a reusable CiAi workflow-call interface that requires an immutable candidate SHA. The workflow exists on PR #13 but not yet on a canonical branch, so it cannot be dispatched/called until integration. Package archive integrity and clean-consumer compatibility remain unverified.
- Ordinary development CI continues to use the FSM_Serialization `development` source and does not pack or publish packages.
- No merge, release, deployment, NuGet publication, canonical-branch write, manual release-verification dispatch, or branch deletion has been performed.

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

- `master` and the current `development` baseline retain the previously published `0.1.0-alpha.1`; the active PR candidate advances to `0.1.0-alpha.2`. Both target .NET 8.
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
