# Markdown.Cache — implementation status

## 2026-11-03 — disk storage foundation

- New independent `net10.0` non-packable Markdown.Cache assembly plus xUnit test project, both registered in `Toolbox.sln`.
- `VerifiedDocumentCache` writes streamed data to a temporary file, enforces a configurable document limit, uses strict UTF-8 and terminal-control validation, SHA-256 content-addressed blobs and an atomically replaced JSON source index.
- Read path verifies source key, length, content hash and complete Unicode/control safety. Failed replacement preserves the last valid indexed copy.
- Initial tests: cross-instance persistence, rejection of unsafe replacements, invalid UTF-8 preservation, tamper rejection, limit rejection, and cancellation preservation.
- **Verification pending:** no dotnet SDK is available in the local execution environment, and no new CI success has yet been observed. Tests have been authored but must not be described as passed.
- Not yet implemented: ETag, HTTP/GitHub providers, TTL, global budget/eviction, cache cleaning, cross-process synchronization, adversarial symlink protection, full crash-durability guarantees, or the `mdview` app.

Next: run and correct Windows/Linux solution CI; then add fault-injection tests, safe per-user root selection and cross-process locking before attaching network providers.

## 2026-11-03 — writer lock and cache directory hardening

- Added a cross-process exclusive `.write.lock` acquired with cancellation-aware retry, in addition to the existing in-instance semaphore. Existing cache reads remain independent and verify the published entry.
- Refuse direct symlink/reparse-point cache roots, blob/index directories and target files (including dangling link checks), and repair digest-named corrupt blobs when writing verified replacement bytes. Remaining TOCTOU and trusted-parent-path limitations are explicitly documented.
- Added `CacheDirectory.GetDefaultPath()` using per-user Windows LocalApplicationData or XDG cache / `~/.cache` on Unix; no directory is implicitly created by this helper.
- Added four tests: independent instances writing one key, corrupt blob replacement, Linux symlinked directory rejection and per-user absolute cache path. **Total authored tests: ten; not executed or CI-confirmed.**
- Local container has no `dotnet` executable and no outward DNS to install it. No new GitHub Actions success was established. Next: CI verification, test fixes, then fault injection, secure root provisioning, stale/conditional metadata and HTTP fake-handler tests.
