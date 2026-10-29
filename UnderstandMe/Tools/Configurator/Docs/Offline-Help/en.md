# Offline Markdown help in UnderstandMe.Configurator

**Implementation:** first offline increment (2026-10-29). Online update service, document AST and full Markdown parsing remain planned.

## Topics and navigation

Help is accessible from the main menu and editor, and contextually from the **Translation providers** and **Dictionary storages** registration menus. Context Help opens the respective registration topic and returns to the same in-memory editor draft; no network or file save takes place.

Topics are authored in `Docs/Help/<topic>/en.md` and embedded into the Configurator executable. The catalog uses stable, nonlocalized IDs; English/Czech menus use the existing `ConfiguratorStrings` resources. Help content currently exists in English only, explicitly indicated in both UI languages.

The initial lightweight, safe terminal presenter formats headings, bullet lists and fenced code. Other Markdown syntax is displayed literally (including inline formatting and links); it does **not** execute HTML or automatically open arbitrary links. A full owned Markdown parser/writer with translation-aware AST, URL protection, locale-aware dual links and structural validation is a separate follow-up. See [broader design](../../../Docs/Markdown-Translation-and-Documentation-Updates/en.md).

## Safety and compatibility

- The embedded help cannot be damaged by a corrupt downloaded update; there is no downloaded cache or update network call in this release.
- Opening Help never modifies `UnderstandMeSettings.json` or Configurator preferences.
- Documentation files are UTF-8 and bundled at build time; adding a topic requires registering its stable ID, localized title key and build resource.
- Escape/Back works from the topic and topic selection menus.
- Headless `validate` remains unaffected.

## Testing

Virtual terminal tests cover the main Help flow and context Help with an unsaved draft. Catalog tests ensure every declared topic has nonempty embedded Markdown and unknown IDs are rejected. Full CI status must be verified after the commit.
