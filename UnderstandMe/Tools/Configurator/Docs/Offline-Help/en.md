# Offline Markdown help in UnderstandMe.Configurator

**Implementation:** embedded offline help + shared Blackboard renderer integration (2026-10-29). Online update service, full CommonMark AST and translation-aware Markdown editing remain planned.

## Topics and navigation

Help is accessible from the main menu and editor, and contextually from the **Translation providers** and **Dictionary storages** registration menus. Context Help opens the respective registration topic and returns to the same in-memory editor draft; no network or file save takes place.

Topics are authored in `Docs/Help/<topic>/en.md` and embedded into the Configurator executable. The catalog uses stable, nonlocalized IDs; English/Czech menus use the existing `ConfiguratorStrings` resources. Help content currently exists in English only, explicitly indicated in both UI languages.

The Configurator now delegates presentation through its small `ConfiguratorHelpRenderer` theme adapter to `Blackboard.SpectreConsole.Markdown`, which consumes `ExchangeHouse.Markdown`. Basic headings, lists, fenced/inline code, bold, italic and simple links are displayed as formatted Spectre text. The original link target is shown next to the label; **no URL is automatically opened**, and Spectre markup is not executed. Unsupported Markdown syntax remains literal. Full CommonMark conformance, translation-aware AST editing, URL protection, localized dual-link generation and structural validation remain separate follow-ups. See [broader design](../../../Docs/Markdown-Translation-and-Documentation-Updates/en.md).

## Safety and compatibility

- The embedded help cannot be damaged by a corrupt downloaded update; there is no downloaded cache or update network call in this release.
- Opening Help never modifies `UnderstandMeSettings.json` or Configurator preferences.
- Documentation files are UTF-8 and bundled at build time; adding a topic requires registering its stable ID, localized title key and build resource.
- Escape/Back works from the topic and topic selection menus.
- Headless `validate` remains unaffected.

## Testing

Virtual terminal tests cover the main Help flow and context Help with an unsaved draft, together with shared inline Markdown presentation, original link destinations and literal code/markup safety. Catalog tests ensure every declared topic has nonempty embedded Markdown and unknown IDs are rejected. Full CI status must be verified after the commit.
