# Configurator.Tests — Implementation Status

## 2026-10-25 — initial suite

- Created xUnit project for Configurator session and Spectre.Console.Testing virtual-terminal smoke checks.
- Added draft lifecycle, validation, conflict, cancellation, and retained provider/storage tests.
- GitHub CI must compile and run before these tests are claimed green.

### 2026-10-25 — picker end-to-end selection

- Added an injected TestConsole keyboard test of selecting a real JSON file through the published FileBrowser and returning to the Configurator; asserts the original file remains unchanged.
- Previous [GitHub CI 40229038875](https://github.com/afrowaveltd/Toolbox/actions/runs/40229038875) passed with **7 Configurator tests** and the full solution build. New picker test requires fresh CI verification.

### Verified (2026-10-25)

- [GitHub Actions 40229277472](https://github.com/afrowaveltd/Toolbox/actions/runs/40229277472) **SUCCESS** with **8 Configurator tests**, including end-to-end virtual-terminal selection of a `settings.json` file through the published FileBrowser.
- Previous `7`-test result was superseded by this integration check. No real SSH or remote provider connectivity testing is claimed.

## 2026-10-25 — bilingual launcher and independent user preferences: 14 green

- Added per-user preferences tests: missing file is not created by read, separate host settings, create-only write, revision conflict, malformed/unsupported settings fail closed.
- Added bundled English/Czech resource parity and offline fallback tests.
- Updated interaction tests for typed main-menu navigation; added UI language persistence/switching, offline Test configuration via real FileBrowser, corrupt preferences fallback and safe editor Back/Exit tests.
- [GitHub Actions #40231883535](https://github.com/afrowaveltd/Toolbox/actions/runs/40231883535): **14/14 Configurator tests passed** and all existing solution test suites passed. Manual SSH/Windows smoke not yet claimed.

### Final green run — 2026-10-25

- [GitHub Actions #40232402628](https://github.com/afrowaveltd/Toolbox/actions/runs/40232402628) **SUCCESS**: 14/14 Configurator tests; 6,217 total tests across the workflow; build warnings/errors 0/0.

## 2026-10-25 — menu clearing and acknowledged result screens

- Expanded to **18** tests: verify ANSI screen-clear after main-menu return and editor-menu refresh, only after acknowledgement of readable summaries/validation findings, and for canceled file browsing and preferences navigation. Corrupted JSON does not get changed.
- [GitHub Actions #40233060410](https://github.com/afrowaveltd/Toolbox/actions/runs/40233060410) **SUCCESS**, full test suite green and 0 build warnings/errors.

## 2026-10-25 — provider/storage editing and optional live HTTP probe

- New `ConfiguratorComponentsTests` cover built-in descriptors, complete-draft dirtiness, add/remove, type/limit validation, conditional secret-reference validation, revision conflicts and schema-safe persistence.
- New `ConfiguratorConnectivityTests` use fake HTTP handlers and no real network to check GET /languages, JSON-model tolerance, rejection of non-2xx or non-JSON, credential-free requests, disabled/unsupported/invalid endpoints, failure never blocking save and optional TUI live test preserving the configuration file.
- The existing localized TUI result acknowledgement is preserved for offline diagnostics and explicit online tests. Latest GitHub Actions verification pending after the last commit.
