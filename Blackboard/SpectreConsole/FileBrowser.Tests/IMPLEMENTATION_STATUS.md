# FileBrowser.Tests — Implementation Status

## 2026-10-22 — initial test suite implemented

- Created independently built .NET 10 xUnit test project and added it to `Toolbox.sln`.
- Added `FileBrowserFileIconsTests`, **37 passing cases in GitHub Actions run 39853748075**, covering name/extension classification, compound extensions, folder flag, instance overrides, invalid inputs, `Auto/On/Off` icons, readable English labels and Unicode-name invariants.
- Verified GitHub Actions workflow build and tests green for commit `0be57bf9` (the complete new project/code/test slice).
- No interactive UI navigation, terminal integration or network-provider tests are claimed: those will be added with their respective implementations.

Next: add navigation and Spectre rendering test seams, then test actual terminal capability detection and SSH/legacy console behavior.

## 2026-10-22 — lazy trees and recursive all-result search tests

- Added `FileBrowserTreeSearchTests` for lazy expansion, same-name matches in multiple branches, pruned result trees, explicit selected-result navigation, no movement on misses, wildcard/filename matching, provider case sensitivity, JSON filters, inaccessible subtrees, cycles, depth/count limits, cancellation and a real local-directory listing/parent regression.
- Initial GitHub Actions runs identified two **test-only defects**: a helper named `File` shadowed `System.IO.File` (`d97c957` fixed), and `config?.json` correctly matched both `config1.json` and `configs.json` (`cc1997a` fixed the expectation).
- The latest CI for these fixes must complete before reporting final totals. The actual interactive Spectre.Console UI and FTP provider are still not part of the test suite.

### Verified

- GitHub Actions [39858309064](https://github.com/afrowaveltd/Toolbox/actions/runs/39858309064) completed successfully: **119 FileBrowser tests passed**, including tree/search behaviors, and the full Toolbox build was successful.

## 2026-10-24 — universal masks and interactive prompt verification

- Added `FileBrowserNameMatcherTests` for OR-combined masks (`*.ajis;*.json`), all-file pattern, exact matches, source case sensitivity, regex alternatives, invalid regex/masks, bounded timeout, integration with listing and recursive search. The first CI exposed that `RegexParseException` is an `ArgumentException` subclass; corrected the test to accept the subtype in commit `a685bc7`.
- Added `Spectre.Console.Testing 0.57.2` as a **test-project-only** dependency and `SpectreFileBrowserInteractionTests` using injected `TestConsole` to simulate Enter, Esc and mask-filtered selection; GitHub Actions [40099094982](https://github.com/afrowaveltd/Toolbox/actions/runs/40099094982) **SUCCESS with 145 passing FileBrowser tests**.
- Further virtual-keyboard tests for selecting the second same-named file in a search result and cancelling a zero-result search were committed in `6821dba` and must be checked separately. Manual SSH/Windows keyboard behavior still requires real-terminal testing.

### Verified interactive multi-match behavior

- [GitHub Actions run 40099273392](https://github.com/afrowaveltd/Toolbox/actions/runs/40099273392) completed successfully with **147 FileBrowser tests**. Included end-to-end `TestConsole` search selecting the second matching filename from a different directory, and a no-result search followed by Esc with no selection.
- This verifies the mocked terminal workflow on Linux CI; actual Windows/SSH/legacy console compatibility remains to be manually tested.

### 2026-10-24 — 148 verified tests

- Added a virtual-terminal regression ensuring a directory that cannot be enumerated is never offered as a successful current-folder selection.
- [GitHub Actions run 40099579962](https://github.com/afrowaveltd/Toolbox/actions/runs/40099579962) **SUCCESS** with **148 FileBrowser tests** and green solution build. Test coverage includes injected Spectre keyboard input; it does not replace manual real-terminal testing.

## 2026-10-25 — real Windows terminal smoke report (manual, not xUnit)

- After updating, the maintainer reported the current interactive FileBrowser scenarios passing in available Windows Terminal shell profiles (Command Prompt, PowerShell, Developer Command Prompt and Git shell).
- The maintainer subsequently started legacy Windows Console Host (`conhost.exe`) directly and confirmed working behavior **including colors**. Running cmd within Windows Terminal alone would not have tested this older console host.
- This is **manual compatibility evidence**, not an additional automated test or an increase in xUnit count. The last explicitly reported local FileBrowser unit-test run remains **160 passed, 0 failed, 0 skipped** (2026-10-24). The latest GitHub Actions run reviewed for the pre-documentation commit is [40121980599](https://github.com/afrowaveltd/Toolbox/actions/runs/40121980599), SUCCESS.
- Debian interactive smoke was reported previously. Real SSH/tmux, edge-case fonts and legacy code pages remain outside this verified matrix. No test source changes in this documentation commit.

## 2026-10-29 — Windows path wrapping regression

- A local Windows run on .NET 11 RC (projects target net10.0) reported failed UI assertions on long temporary paths. Spectre.Console.Testing wraps paths at the default virtual width; it does not imply selection/navigation failed.
- The specific tests asserting an unbroken full path now set `console.Profile.Width = 512`; other default-width navigation tests remain unchanged.
- Added a new `windows-ui` GitHub Actions job for FileBrowser, Configurator and Showcase tests. Windows CI verification pending at this commit; no PASS claim yet.

### Verified on both platforms — 2026-10-29

- [GitHub Actions #40857564421](https://github.com/afrowaveltd/Toolbox/actions/runs/40857564421) **SUCCESS** for code/test commit `ae52a29345f0fcbef08cbf6cda41686461bc25bd`.
- Ubuntu full solution restore/build/test **PASS**; new Windows runner's **FileBrowser**, **UnderstandMe Configurator**, and **Blackboard Showcase** project test steps all **PASS**.
- The original Windows UI path-wrap failures are resolved in the automated Windows environment. Manual developer-local retest with the latest checkout remains distinct from Windows CI.
