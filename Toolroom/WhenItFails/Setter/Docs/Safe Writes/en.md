# Safe Writes

All write operations in the WhenItFails Setter follow a safe write protocol designed to prevent data loss and corruption.

## Protocol

Every modification to a JSON catalog file goes through these steps:

1. **Load** — The target file is read and parsed into a strongly-typed document model.
2. **Locate** — The specific item to modify is found by its identifier.
3. **Modify** — The change is applied to the in-memory model.
4. **Validate** — The entire modified document is re-validated against all catalog rules (schema, cross-references, code ranges, etc.).
5. **Write temp** — If validation passes, the document is serialized to a temporary file in the same directory.
6. **Backup** — The original file is copied to a timestamped backup (e.g., `errors.en.json.20260611_230800.bak`).
7. **Replace** — The temporary file is atomically moved to replace the original.

## Guarantees

- **Atomic replacement** — The target file is never partially written. Either the old file remains intact, or the new file fully replaces it.
- **Validation gate** — Invalid changes are rejected before any file I/O occurs. The original file is never touched if validation fails.
- **Timestamped backups** — Every successful write creates a backup. Old backups accumulate and can be cleaned up manually.
- **Rollback on failure** — If any step after validation fails (e.g., disk full), the in-memory change is reverted and the original file is preserved.

## Current Write Commands

| Command | What it modifies | File |
|---|---|---|
| `set-title` | Error definition title | `errors.en.json` |

Future commands (e.g., `set-message`, `set-severity`) will follow the same safe write protocol.

## Backup Files

Backup files use the pattern `<original-filename>.<timestamp>.bak`. For example:

```
errors.en.json.20260611_230800.bak
errors.en.json.20260610_154200.bak
```

These are plain copies of the original file before modification. They are not managed automatically — users should periodically clean up old backups.

> Localized versions of this documentation may be generated later by Afrowave translation tooling.
