# Markdown.Cache — implementation status

## 2026-11-03 — disk storage foundation

- New independent `net10.0` non-packable Markdown.Cache assembly plus xUnit test project, both registered in `Toolbox.sln`.
- `VerifiedDocumentCache` writes streamed data to a temporary file, enforces a configurable document limit, uses strict UTF-8 and terminal-control validation, SHA-256 content-addressed blobs and an atomically replaced JSON source index.
- Read path verifies source key, length, content hash and complete Unicode/control safety. Failed replacement preserves the last valid indexed copy.
- Initial tests: cross-instance persistence, rejection of unsafe replacements, invalid UTF-8 preservation, tamper rejection, limit rejection, and cancellation preservation.
- **Verification pending:** no dotnet SDK is available in the local execution environment, and no new CI success has yet been observed. Tests have been authored but must not be described as passed.
- Not yet implemented: ETag, HTTP/GitHub providers, TTL, global budget/eviction, cache cleaning, cross-process synchronization, adversarial symlink protection, full crash-durability guarantees, or the `mdview` app.

Next: run and correct Windows/Linux solution CI; then add fault-injection tests, safe per-user root selection and cross-process locking before attaching network providers.
