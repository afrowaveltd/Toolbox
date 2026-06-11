# Setting Title — Safe Write Behavior

The `set-title` command modifies an error definition's title in the `errors.en.json` file. It uses a safe write pipeline to prevent data corruption.

## Write Pipeline

1. **Load JSON** — The error catalog file (`errors.en.json`) is loaded and parsed.
2. **Find error** — The target error definition is located by id, numeric code, or name.
3. **Modify value** — The title field is updated in memory.
4. **Validate** — The entire modified document is re-validated against all catalog rules.
5. **Write temp file** — If validation passes, the document is written to a temporary file.
6. **Create timestamped backup** — The original file is copied to a backup with a timestamp suffix (e.g., `errors.en.json.20260611_230800.bak`).
7. **Replace target file** — The temporary file is atomically moved to replace the original.

## Rollback on Failure

If validation fails after the title change, the in-memory title is reverted to its original value and the file is **not** written. The command reports the validation errors so the user can fix underlying issues before retrying.

If the file save operation fails (e.g., disk full, permission denied), the original file is left untouched and the temporary file is discarded.

## Example

```
when-it-fails-setter set-title . AFW_NET_0001 "Network unavailable"
```

Output on success:

```
Updated title: AFW_NET_0001
New title: Network unavailable
Error title changed from 'Network unavailable' to 'Network unavailable'.
```

> Localized versions of this documentation may be generated later by Afrowave translation tooling.
