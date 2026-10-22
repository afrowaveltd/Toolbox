# Blackboard — Implementation Status

## 2026-10-22 — Reserved project family, architecture only

- Established **Blackboard** as proposed reusable TUI widget family, pairing with the prospective GUI **Whiteboard** family (name not frozen).
- Confirmed planned first adapter `SpectreConsole`, starting with reusable `FileBrowser` based on ideas from the author's earlier Lutonet file picker.
- Documented boundaries with `Toolroom`, `SeeMe`, `UnderstandMe` and future Avalonia components. Specific settings editing belongs in a Toolroom application; widgets remain application-independent.
- Created [README](README.md), [architecture document](Docs/Architecture/en.md) and [component outline](SpectreConsole/FileBrowser/README.md).

**Current status:** no `.csproj`, C# implementation, NuGet artifact, tests or solution registration in this step. Names/package boundaries are open for refinement before runtime implementation.

### Next suggested work

1. Confirm naming (Blackboard/Whiteboard pair).
2. Implement framework-independent file navigation and a Spectre.Console adapter with modern stable package dependencies; avoid process-CWD changes.
3. Add CLI/SSH-friendly cancellation, filtering, safe markup, localization injection and error handling.
4. Add automated tests and include real projects in `Toolbox.sln`; check GitHub Actions.
5. Integrate the picker into the future UnderstandMe Configurator using the existing validated JSON settings store.
