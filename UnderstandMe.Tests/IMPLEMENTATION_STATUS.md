# UnderstandMe.Tests — Implementation status

## Repository baseline, inspected 2026-10-29

- This is a .NET 10 xUnit project for UnderstandMe core; existing tests already cover localization lookup/fallback, placeholder safety and several core settings and localization services. Older CI runs are documented in `../UnderstandMe/IMPLEMENTATION_STATUS.md`.
- Added this per-project continuation file, README and localization regression topic documentation to follow the Toolbox project conventions.

## 2026-10-29 — Windows dictionaryless concurrency correction

- Original `DictionaryTextLocalizerTests.Concurrent_registration_does_not_lose_keys` failed locally under Windows (24 persisted keys, expected 25).
- Kept the original regression and added concurrent registration via 60 independent store instances and 48 phrases over four independent localizers.
- Corresponding source implementation uses a directory-level named mutex and bounded whole-transaction retries. Source and tests are committed as `9d8aac3e`.
- [GitHub Actions #40859339217](https://github.com/afrowaveltd/Toolbox/actions/runs/40859339217) **SUCCESS**: full Linux solution and targeted Windows localization regressions, together with Windows UI suites.
- A future stress/chaos suite should explicitly inject sharing violations, persistent write errors, process crashes and distributed-storage race conditions; the current test suite does **not** prove those scenarios safe.

### Next

Continue ExchangeHouse / Blackboard integration separately; keep localization tests in Windows CI as a permanent guard.
