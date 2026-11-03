# TextFiles — implementation status

## 2026-11-03 — extraction from Showcase

- Created `Blackboard/TextFiles` as an independent `net10.0` library, with the original strict `TextFilePreviewReader`, `TextPreviewKind` and `TextPreviewResult` implementation moved from `Blackboard/Samples/Blackboard.Showcase`; namespace changed to `Afrowave.Toolbox.Blackboard.TextFiles`.
- Original sample copy removed. Both `MdView` and `Blackboard.Showcase` now use normal `ProjectReference` and importing the new namespace, with no cross-project `Compile Include` and no executable dependency.
- Updated Showcase JSON preview, text preview, Markdown preview and text/JSON tests to consume the extracted public types. Existing behavior intended unchanged.
- Added `Blackboard/TextFiles.Tests` with five independent tests (unicode, bounded prefix, unsafe terminal character beyond prefix, UTF-16 BOM, invalid UTF-8).
- Both projects registered in `Toolbox.sln`.
- **Tests not yet rerun for this extraction**; the user-confirmed 6,438-test Windows pass refers to the preceding commit. Current compile/test success is not yet verified. Next: run full solution tests and ensure no namespace/solution setup regressions; then work on user-visible `mdview` FileBrowser selection and paging.

## 2026-11-03 — Windows test log review: missing test project

- User's second Windows `dotnet test Toolbox.sln` output confirms the new TextFiles **library builds** and the existing suites pass, including `MdView.Tests` 10/10 and Showcase.Tests 39/39.
- **Important:** `TextFiles.Tests` was absent from the executed assembly list. The output reports only 27 project restore entries while `Toolbox.sln` contains 28 C# projects; the five new reader-specific tests therefore are **not confirmed passed**.
- Corrected the solution's project configuration mapping for both TextFiles projects by adding `Debug|x64`, `Debug|x86`, `Release|x64`, `Release|x86` ActiveCfg and Build.0 entries, consistent with other solution projects. Missing mappings are a likely reason the tests were skipped under a non-Any-CPU solution platform; this is not yet independently confirmed.
- Required follow-up: rerun `dotnet test Toolbox.sln` and check specifically that `Afrowave.Toolbox.Blackboard.TextFiles.Tests.dll` executes **5/5**. A direct project test command is also useful: `dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj`.
