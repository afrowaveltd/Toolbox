# Blackboard.Showcase.Tests — Implementation Status

## 2026-10-24 — initial automated tests

- Created an independent .NET 10 xUnit project referencing only the Showcase and the stable Spectre.Console.Testing runtime.
- Added tests for text previews (valid Unicode, binary, invalid UTF-8, unsafe controls, 16 KiB read bounding, empty), JSON previews (valid/invalid/oversize/binary/empty), and the modular menu/JSON-specific picker workflow via virtual-keyboard TestConsole.
- Registered the test project in Toolbox.sln with the other Blackboard and Toolbox projects.
- GitHub Actions result must be verified after these files are committed; the tests have not been manually validated in real terminals.

Next: confirm CI and add regression cases for any Windows/SSH differences that appear during live testing.

## 2026-10-24 — first 18 tests verified

- GitHub Actions [40108814777](https://github.com/afrowaveltd/Toolbox/actions/runs/40108814777) **SUCCESS**: all **18 Showcase tests** passed, all 148 FileBrowser tests stayed green, and the entire solution built successfully.
- Coverage includes modular menu navigation, JSON-only filtering and colored JSON preview, invalid JSON rejection, safe text encoding and control-character rules, 16 KiB preview bounds and the 64 KiB JSON ceiling. Real-terminal manual testing is still needed.

### 2026-10-24 — 20 tests fully verified

- Added two `Spectre.Console.Testing` tests of `FileBrowserPreviewDemo` and `FileBrowserDirectoryDemo`, including actual file selection via Enter, Unicode text output and no filesystem modification.
- [GitHub Actions 40109040501](https://github.com/afrowaveltd/Toolbox/actions/runs/40109040501): **SUCCESS**, **20 Showcase tests**, **148 FileBrowser tests** and successful full Toolbox solution build. All Showcase scenarios now have direct virtual-terminal coverage.
- Manual real Windows/SSH/legacy-terminal acceptance testing is still pending.

## 2026-10-29 — Windows path wrapping regression

- A local Windows run on .NET 11 RC (projects target net10.0) reported failed UI assertions on long temporary paths. Spectre.Console.Testing wraps paths at the default virtual width; it does not imply selection/navigation failed.
- The specific tests asserting an unbroken full path now set `console.Profile.Width = 512`; other default-width navigation tests remain unchanged.
- Added a new `windows-ui` GitHub Actions job for FileBrowser, Configurator and Showcase tests. Windows CI verification pending at this commit; no PASS claim yet.

### Verified on both platforms — 2026-10-29

- [GitHub Actions #40857564421](https://github.com/afrowaveltd/Toolbox/actions/runs/40857564421) **SUCCESS** for code/test commit `ae52a29345f0fcbef08cbf6cda41686461bc25bd`.
- Ubuntu full solution restore/build/test **PASS**; new Windows runner's **FileBrowser**, **UnderstandMe Configurator**, and **Blackboard Showcase** project test steps all **PASS**.
- The original Windows UI path-wrap failures are resolved in the automated Windows environment. Manual developer-local retest with the latest checkout remains distinct from Windows CI.

## 2026-10-29 — Markdown theme gallery regressions

- Added six virtual terminal tests for `MarkdownThemeDemo` and host dispatch. Existing FileBrowser/JSON tests were not modified.
- New demo uses the already verified `Blackboard.SpectreConsole.Markdown` API and a shared parsed document, not a replacement for ExchangeHouse.
- Linux + Windows verification pending; do not report the new tests as green until CI completes.

### Cross-platform CI verification — 2026-10-29

- [GitHub Actions #40876131365](https://github.com/afrowaveltd/Toolbox/actions/runs/40876131365) **SUCCESS** on implementation commit `f711e66538941118592ff6d1221bf5b7f0aff57d`.
- Linux: full Toolbox solution restore, build, and test **passed** (includes the new Markdown theme-gallery unit/virtual-terminal tests).
- Windows: FileBrowser, UnderstandMe Configurator, Blackboard Showcase (including the new gallery), and UnderstandMe localization concurrency suites **passed**.
- Manual interactive visual confirmation on an actual Windows/Debian terminal is still outstanding; automatic tests do not certify terminal font/theme appearance.
