# Branch Reconciliation Ledger

**Review date:** 2026-10-22 UTC  
**Scope:** `master` versus `development`  
**Disposition:** Audit only. No merge, cherry-pick, reset, or branch deletion has been performed.

## Current branch evidence

- `development`: `eac7f44562a4074bb3eac0d30df35861ecb6114c`
- `master`: `d4aabc04c1f6e34c926a37b5340b62de0fc5c3d9`
- GitHub comparison: `development` is 18 commits ahead and 8 commits behind `master`.
- No open pull requests were reported during this review.
- Latest CI for the current development head: [run #39717094485](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/39717094485) — passed.

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

- Both branches declare `0.1.0-alpha.1` and target .NET 8.
- Development includes Markdown under `docs/` and SVG assets in the NuGet package. Master packs only the root README in the project file. Keep the development package-documentation behavior.
- Development's publication job uses `if: ${{ false }}`; master uses a master-branch condition combined with `false`. Neither is currently permitted to publish.
- Development pins NuGet Trusted Publishing's action to an immutable commit SHA; master uses a mutable version tag.
- The current head's package artifact and documentation assets have passed the CI audit. The latest documentation-only commit also passed [run #39717094485](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/39717094485).

## Required before declaring branch reconciliation complete

- [ ] Compare the master-only README additions with the development README and preserve useful identity/standard content.
- [ ] Verify all provider adapters and their failure/security contracts against source and tests.
- [ ] Review the full workflow history; never restore automatic publication.
- [ ] Confirm the package's docs and SVG assets remain in the artifact.
- [ ] Re-run CI on the exact reconciled candidate.
- [ ] Obtain repository-owner approval before merging or deleting any branch.

**This ledger is not authorization to delete `master` or any other branch.**
