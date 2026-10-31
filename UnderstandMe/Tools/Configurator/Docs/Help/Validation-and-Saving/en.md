# Validation and saving

Changes to providers, storages, source language and queue buffer capacity stay in a draft until you explicitly save.

- **Validate draft** checks the draft using the shared UnderstandMe settings validator.
- **Review & Save** first displays a non-secret summary and asks for confirmation.
- A failed validation does not overwrite the destination file.
- If another process changed the original file, revision checking refuses a stale save.
- **Test configuration (offline)** validates an existing file without modifying it.

Unsaved changes are preserved when you visit Help. Returning from the editor asks before discarding them. A live provider connectivity failure is advisory, not a save blocker.
