# Spectre.Console Markdown rendering and themes

**Status:** first renderer with limited syntax, not full CommonMark and not yet published.

This adapter reads `MarkdownDocument` nodes from ExchangeHouse and renders through an injected `IAnsiConsole`. It never parses raw input as Spectre markup, opens links, downloads remote content or mutates the original document. Its `Render` overloads accept Markdown text or a previously parsed document, so theme changes do not trigger reparsing.

## Theme

`MarkdownSpectreTheme` exposes all six heading styles and configurable body, strong, emphasis, code, links, link destinations and list marker styles. It also configures the list glyph, code indentation, and whether to show original code fence markers or link destinations. All style configuration is optional; Spectre.Console determines color fallback from terminal capabilities.

The renderer currently uses the innermost inline style without combining all nested decorations. Input that ExchangeHouse does not recognize is displayed as ordinary text instead of being erased. Terminal escape/control characters are replaced; brackets and raw HTML remain literal.

## Roadmap

- CI and real Windows/Debian terminal smoke
- Optional Blackboard.Showcase rendering/theme demo
- Switch UnderstandMe Configurator Help to this reusable renderer with regression tests
- Broaden ExchangeHouse syntax support before adding advanced rendering
