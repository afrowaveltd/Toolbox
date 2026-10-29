# File JSON dictionaries — concurrent registration and atomic updates

## Observed Windows regression

On 2026-10-29, the Windows test `DictionaryTextLocalizerTests.Concurrent_registration_does_not_lose_keys` failed with 24 persisted keys where 25 were expected. The same test passed in earlier Linux CI. The test uses a shared localizer, which already serializes its own lookups, so a temporary file I/O error (caught for best-effort fallback) is a possible direct trigger. Independently, the file store had only per-instance synchronization and could lose updates when separate store instances or processes concurrently edited one directory.

## Updated storage contract

- All `FileJsonDictionaryStore` operations — `Load`, `TryAdd`, `TryRemove`, and conditional `TryPublish` — now use a stable named OS mutex derived from the canonical directory path (hashed, not exposed). A single directory-level transaction lock also serializes source/target validation and publication without lock-order deadlocks. Different directories remain independent.
- Each mutation rereads the latest on-disk JSON **under the mutex** before writing to a unique temporary file and atomically replacing the destination. A failed transaction is retried from the beginning, never using a stale in-memory copy.
- Up to five whole-transaction attempts are permitted for `IOException`, with bounded delays between them (10, 40, 90, 160 ms). This handles transient file-sharing/antivirus activity; it does not mask persistent disk errors. Invalid JSON and unauthorized access do not trigger retries or overwrites. A lock-wait timeout raises an `IOException`.
- A process killed during an update releases its OS mutex; the previous JSON remains safe thanks to atomic replacement, and orphaned temporary files can be cleaned separately. Host-specific remote/network file systems might not honor the same OS mutex namespace across machines; distributed storage requires a different backend.

## Localization behavior and guarantees

`DictionaryTextLocalizer` still returns the original source string if registration fails on a read-only or unavailable store. That fallback preserves UI availability but does **not** promise persistence under permanent storage failure. The implementation still catches expected I/O, permissions and malformed JSON without overwriting source dictionaries. No public API changes or extra NuGet dependencies were made.

## Regression coverage

- Existing 25-key parallel registration on one localizer.
- New parallel registration through 60 independent file-store instances.
- New parallel registration through 4 separate localizers for 48 phrases.
- Existing validation for duplicate keys, malformed JSON, conditional invalidation, source template and target translation integrity.

Linux solution CI and the targeted Windows UnderstandMe localization tests must both pass before this increment is marked verified.

### Verified CI — 2026-10-29

- [GitHub Actions #40859339217](https://github.com/afrowaveltd/Toolbox/actions/runs/40859339217) **SUCCESS** for implementation commit `9d8aac3e3469a56a7e6e150d7baf9e9f8031ba2d`.
- Linux: full solution restore/build/test passed. Windows: FileBrowser, Configurator, Showcase and targeted `DictionaryTextLocalizerTests` passed, including the prior 25-key regression and the 60-/48-key independent-instance tests.
- This establishes green CI for the current run, not a guarantee that permanent filesystem failures can never prevent persistence. Local user rerun on the original workstation remains a separate verification.
