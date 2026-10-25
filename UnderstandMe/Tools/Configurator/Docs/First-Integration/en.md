# First FileBrowser integration — UnderstandMe Configurator

2026-10-25, first executable slice.

## Structure and guarantees

- `UnderstandMe/Tools/Configurator` uses the **published** `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser 0.1.0-preview.1` via NuGet PackageReference and `UnderstandMe` via ProjectReference.
- FileBrowser selects existing JSON paths using an initial `UnderstandMeSettings.json;*.json` filter. Picker cancel performs no write. No global working-directory changes.
- The core `UnderstandMeSettingsJsonStore` parses strict JSON, validates, remembers SHA-256 revision and saves with checked revision and safe local file replacement. An unsupported schema or unknown adapter type cannot silently round-trip as a default configuration.
- A `ConfiguratorSession` holds a RAM draft. Only default source language and worker FIFO capacity are editable in the first implementation; other registrations are preserved unchanged. The overview avoids printing provider options or secrets.
- Review & Save displays the target and edited non-sensitive fields, requires confirmation and re-validates, then calls the core store; errors retain the draft.
- Exiting/cancelling is read-only; no automatic save on shutdown.
- A noninteractive `validate --config <path>` mode returns meaningful shell exit codes.

## Future

Design-level features from [Configurator TUI](../../../Docs/Configurator-TUI/en.md) remain planned: metadata-driven backend/storage options, new/delete/enable, Ctrl+S / Ctrl+V shortcuts, undo/redo, rich contextual help and full draft diff, localized menu labels, discovery across bounded paths, SSH/tmux and legacy console testing.

## Verification

The sibling xUnit project covers draft creation/save, persistence conflict, unchanged files on invalid/cancel, preserving existing registrations and virtual-terminal cancellation. CI must be inspected before marking the first milestone green.
