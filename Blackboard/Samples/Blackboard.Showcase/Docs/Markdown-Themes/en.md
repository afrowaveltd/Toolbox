# Markdown theme gallery

**First implementation: 2026-10-29.** This topic documents the separate, reusable Spectre renderer as demonstrated by Blackboard.Showcase, not a new parser implementation or a new NuGet package.

## Try it

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
```

Choose **Markdown — preview and compare color themes** in the fourth Showcase menu position. Choose **Classic**, **Ocean**, **Forest**, **Monochrome**, or **Compare all themes**. After preview, press Enter to return to the gallery; Escape goes back to the Showcase. The sample text is bundled into the demo; no file selection or network connection is needed.

The preview intentionally contains headings, strong/emphasis, a simple link with a relative destination, a list and a fenced C# example. The **same ExchangeHouse `MarkdownDocument` instance** is used for all theme variants, so changing colors does not reparse or change the source. All themes show the link destination as text and never activate it.

## Dependency and ownership boundaries

- `ExchangeHouse.Markdown` parses Markdown and retains original source positions and text.
- `Blackboard.SpectreConsole.Markdown` renders the document through an injected `IAnsiConsole`, accepting a `MarkdownSpectreTheme` with independently configurable headings, strong/italic/code/link styles, bullet and indentation options.
- `Blackboard.Showcase` **only** registers the gallery and creates four example themes. It is an executable sample, not a reusable library.
- `FileBrowser` never references Markdown or Showcase. The Configurator still uses its existing help viewer; integration is a future independently tested step.

## Theme comparison

- **Classic**: built-in color defaults.
- **Ocean**: blue headings, bold text and link accents.
- **Forest**: green heading/link/bullet accents with compact code indentation.
- **Monochrome**: no explicit foreground colors; relies on bold/underline/italic and ASCII list markers, with code fence markers visible.

The actual appearance depends on terminal color and Unicode support; the sample does not force output encoding, change terminal profile, save configuration or alter the working directory. The renderer is intentionally a limited Markdown viewer, **not yet CommonMark-complete**.

## Automated checks

`MarkdownThemeDemoTests` cover Back/Escape, one theme, switching, all-themes comparison, dispatch through the Showcase host and cancellation. Tests use `Spectre.Console.Testing` (virtual terminal) with no filesystem actions. Windows UI and Linux solution CI must both pass before declaring the increment verified.

### Cross-platform CI verification — 2026-10-29

- [GitHub Actions #40876131365](https://github.com/afrowaveltd/Toolbox/actions/runs/40876131365) **SUCCESS** on implementation commit `f711e66538941118592ff6d1221bf5b7f0aff57d`.
- Linux: full Toolbox solution restore, build, and test **passed** (includes the new Markdown theme-gallery unit/virtual-terminal tests).
- Windows: FileBrowser, UnderstandMe Configurator, Blackboard Showcase (including the new gallery), and UnderstandMe localization concurrency suites **passed**.
- Manual interactive visual confirmation on an actual Windows/Debian terminal is still outstanding; automatic tests do not certify terminal font/theme appearance.
