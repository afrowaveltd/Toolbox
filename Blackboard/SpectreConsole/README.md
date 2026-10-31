# Blackboard — Spectre.Console components

This folder groups **implemented, independent UI component projects**, not a single `Blackboard.SpectreConsole` NuGet package. Each consumer references only the component it needs.

| Component | Status | Documentation |
|---|---|---|
| FileBrowser | Published `0.1.0-preview.1` on NuGet | [README](FileBrowser/README.md) |
| Markdown | Implemented source package `0.1.0-preview.1`, not published | [README](Markdown/README.md) |

FileBrowser is a file/folder picker with wildcard/regex filters, tree/search and testable navigation. Markdown consumes a UI-independent ExchangeHouse document model and renders it using Spectre.Console themes, with text alternatives for images and preservation of original ordered list markers.

Both are demonstrated in [Blackboard.Showcase](../Samples/Blackboard.Showcase/README.md). The Markdown component also powers [UnderstandMe Configurator Help](../../UnderstandMe/Tools/Configurator/README.md). Neither library depends on its applications.

Other widgets (editable forms, status bars and reusable confirmation dialogs) are future possibilities, not existing projects. See [Blackboard architecture](../Docs/Architecture/en.md) and [component packaging](../Docs/Independent-Packaging/en.md).
