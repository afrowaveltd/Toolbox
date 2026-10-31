# Documentation link checker — Implementation status

## 2026-10-31 — initial repository-wide link audit tool

- Added stdlib-only Python scanner over root/project Markdown and English topic docs.
- Validates relative path existence and reports file/line; ignores external targets, code fences and most inline code.
- Registered separate GitHub Actions documentation job. CI verification pending until the first run completes.
- Known limitations: not full CommonMark/reference links, heading anchors or external HTTP status.
