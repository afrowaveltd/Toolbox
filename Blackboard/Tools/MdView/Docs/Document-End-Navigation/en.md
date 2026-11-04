# Document completion: Enter to menu, Escape to exit

**Implemented:** 2026-11-04. This is a small navigation foundation for future in-document search and visual paging.

## Behavior

When `mdview` has displayed a **complete rendered Markdown document** in an interactive terminal, it now remains open and shows:

```text
Enter: main menu  |  Esc: exit
```

- **Enter** opens the existing Spectre.Console main source menu. You can select another local Markdown file, HTTPS URL or GitHub README without restarting the executable.
- **Escape** ends the application successfully (`0`).
- All other keys are ignored. In particular `F`, `PageUp`, `PageDown`, and arrow keys are reserved for future features; they do **not** search, scroll or trigger other actions in this increment.

Both ways of opening Markdown are supported: starting without command-line arguments and entering directly with `mdview local README.md`, `mdview git OWNER REPOSITORY` or `mdview url HTTPS_URL`. Even an empty local Markdown file gives the completion choice after the empty-document notice.

After Enter, the existing source picker clears the terminal and displays the main menu. The document's downloaded bytes/cache remain intact. Every new document is read and rendered using the existing adapters and Markdown/TextMate logic.

## Noninteractive and special cases

The viewer never waits for a key when stdin **or** stdout is redirected, or when Spectre.Console identifies a noninteractive terminal. Thus `mdview local README.md > output.txt` remains a one-shot CLI operation instead of blocking a script. The `--help` command exits immediately as before.

An error opening a source preserves its existing nonzero exit status and does not display the completion prompt. Cancelling GitHub repository selection before opening a README does not display it either. The separate `mdview source PATH` raw-source pager already has its own navigation controls and is deliberately not shown a second completion prompt.

## Architecture

- `MdViewDocumentCompletionPrompt` reads one key at a time through `IAnsiConsole.Input.ReadKeyAsync(intercept: true)` and maps only Enter/Escape to actions; unexpected keys do nothing.
- `MdViewSession` coordinates source selection, display, completion and re-entry into the main menu. Its async delegates separate the workflow from network requests, disk I/O, and rendering.
- `MdViewDisplayResult` distinguishes an actual document display from a cancelled source selection, avoiding a needless keyboard prompt when no document was displayed.
- `Program.cs` preserves the existing source adapters, strict encoding validation, offline cache, TextMate colors and error codes. Resource-owning HTTP clients are disposed before the next document is selected.

This remains **end-of-document navigation**, not an actual Markdown pager. Large Markdown still renders as a complete document; `mdview source` retains its distinct disk-indexed raw line pager.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

New `TestConsole` regressions cover Enter, Escape, ignoring other keys, noninteractive completion, a full Enter → menu → second document → Escape flow, cancellation after returning to the menu, unrendered sources, error codes and avoiding duplicate prompts in raw-source mode.

**The new changes have been committed but not yet compiled or run in this environment** (no local .NET SDK). The most recent maintainer-confirmed full Windows test checkpoint was all green before this feature. Run the tests after pulling before declaring this revision green.

[CLI guide](../CLI/en.md) · [mdview README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).

## Reserved keys and future indexed search (design update 2026-11-04)

A later `F` action will open a topic/phrase selector, populated by a small configurable English default set and real headings. `N` and `P` will navigate through indexed occurrences; novel queries will be scanned on demand and stored against a content digest. **No behavior changes in this increment:** all keys besides Enter and Escape remain ignored after rendering. See [Indexed Search Design](../Indexed-Search/en.md).
