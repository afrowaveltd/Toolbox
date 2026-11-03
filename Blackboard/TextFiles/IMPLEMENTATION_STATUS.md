# TextFiles — implementation status

## 2026-11-03 — extraction from Showcase

- Created `Blackboard/TextFiles` as an independent `net10.0` library, with the original strict `TextFilePreviewReader`, `TextPreviewKind` and `TextPreviewResult` implementation moved from `Blackboard/Samples/Blackboard.Showcase`; namespace changed to `Afrowave.Toolbox.Blackboard.TextFiles`.
- Original sample copy removed. Both `MdView` and `Blackboard.Showcase` now use normal `ProjectReference` and importing the new namespace, with no cross-project `Compile Include` and no executable dependency.
- Updated Showcase JSON preview, text preview, Markdown preview and text/JSON tests to consume the extracted public types. Existing behavior intended unchanged.
- Added `Blackboard/TextFiles.Tests` with five independent tests (unicode, bounded prefix, unsafe terminal character beyond prefix, UTF-16 BOM, invalid UTF-8).
- Both projects registered in `Toolbox.sln`.
- **Tests not yet rerun for this extraction**; the user-confirmed 6,438-test Windows pass refers to the preceding commit. Current compile/test success is not yet verified. Next: run full solution tests and ensure no namespace/solution setup regressions; then work on user-visible `mdview` FileBrowser selection and paging.
