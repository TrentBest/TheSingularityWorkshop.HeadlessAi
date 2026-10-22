<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# Branch Reconciliation Ledger

**Review date:** 2026-10-22 UTC  
**Scope:** `master` versus `development`  
**Disposition:** Audit and documentation reconciliation only. No merge, cherry-pick, reset, or branch deletion has been performed. NuGet publication remains hard-disabled.

## Current branch evidence

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

- Both branches declare `0.1.0-alpha.1` and target .NET 8.
- Development includes Markdown under `docs/` and SVG assets in the NuGet package. Master packs only the root README in the project file. Keep the development package-documentation behavior.
- Development's publication job uses `if: ${{ false }}`; master uses a master-branch condition combined with `false`. Neither is currently permitted to publish.
- Development pins NuGet Trusted Publishing's action to an immutable commit SHA; master uses a mutable version tag.
- The current head's package artifact and documentation assets have passed the CI audit. The latest documentation-only commit also passed [run #39717094485](https://github.com/TrentBest/TheSingularityWorkshop.HeadlessAi/actions/runs/39717094485).

## Required before declaring branch reconciliation complete

- [ ] Compare the master-only README additions with the development README and preserve useful identity/standard content.
- [ ] Refresh exact branch SHAs and ahead/behind counts after the documentation-only follow-up commits; verify CI for the resulting head.
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
