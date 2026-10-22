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
