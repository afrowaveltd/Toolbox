# FileBrowser.Sample — Implementation Status

## 2026-10-24 — initial demo

- Added small .NET 10 executable that demonstrates the standalone local FileBrowser picker in an interactive terminal.
- Supports choosing files or folders; icon overrides, initial custom masks such as `*.ajis;*.json`, or regex such as `\.(ajis|json)$`. Interactive filters, tree navigation, recursive searching and multi-match selection are implemented in the consumed library.
- No production/domain settings logic, translator dependencies, FTP or GUI libraries included.
- CI verification and manual terminal smoke tests are still required; no claim of runtime UI testing is made from a compilation alone.
