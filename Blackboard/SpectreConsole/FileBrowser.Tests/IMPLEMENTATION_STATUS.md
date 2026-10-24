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
