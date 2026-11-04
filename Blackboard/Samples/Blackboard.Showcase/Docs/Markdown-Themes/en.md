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

## 2026-10-31 — nested lists in the sample

The built-in sample now includes a numbered sublist and a nested bullet for testing list indentation and preservation of original numbering with every theme. The rest of the gallery and FileBrowser demos remain unchanged.

## 2026-10-31 — list continuation demo

The same pre-parsed document now includes indented continuation lines under ordered steps and nested list items. Their text is displayed aligned beneath the owner rather than acquiring an extra bullet. See [renderer continuation behavior](../../../../SpectreConsole/Markdown/Docs/List-Continuation-Presentation/en.md).

## 2026-10-31 — loose-list paragraph preview

The embedded Markdown gallery includes an item with two paragraphs separated by a blank line. In every theme, the second paragraph remains aligned beneath the same item's text without an extra bullet. This is still a controlled subset of Markdown; see [renderer notes](../../../../SpectreConsole/Markdown/Docs/Loose-List-Presentation/en.md).

## 2026-11-01 — larger legacy console application code fence

The top-level `csharp` example is now a classic .NET Core 3.1-era
`Program.Main(string[] args)` console application with a named namespace,
nested braces, an `if/else` and a `for` loop. It deliberately illustrates
C# syntax highlighting and indentation over several nesting levels; no
.NET Core 3.1 runtime or SDK is used or required.

The gallery still parses a single immutable source document and renders
it with Classic, Ocean, Forest and Monochrome themes. Tests check that the
rendered method signature and indentation are preserved. Code is never executed.

## 2026-11-01 — quoted list with fenced C#

The single parsed Markdown fixture now also includes `public static int Double(int value)` inside an indented `csharp` fence attached to a numbered item in a blockquote. This shows combined quote/list/code indentation and optional TextMate coloring in Classic, Ocean and Forest. Monochrome stays literal, and no code is executed. [Quoted code renderer](../../../../SpectreConsole/Markdown/Docs/Quoted-Code-Presentation/en.md).

## 2026-11-01 — blank-separated paragraphs in a quoted list

The numbered `13)` quoted item now has an explicit quote-prefixed blank line before its second indented paragraph. The same item then contains the C# fence. This demonstrates loose quoted-list ownership, retained quote prefix, original numbering and source-preserving alignment across every preset. [Quoted loose list renderer](../../../../SpectreConsole/Markdown/Docs/Quoted-Loose-Presentation/en.md).

## 2026-11-01 — list → quote → list → code

The gallery demonstrates a deployment step containing a quoted warning, numbered substep and literal `bash` fence. Its source is parsed once and rendered with three independent indentation layers in all four themes. TextMate remains optional and Monochrome remains plain. [Layout guide](../../../../SpectreConsole/Markdown/Docs/Quoted-List-Containers/en.md).

## 2026-11-04 — multi-language code examples

The gallery's sample source now includes JavaScript, JSON and YAML code fences as well as the original C# demonstration, all using the existing optional TextMate highlighter. The adapter now resolves all bundled language IDs and extensions, not a finite C#-centric list. Unsupported or unavailable grammars still render as safe literal code; the editor/preview never evaluates it. See [supported grammar details](../../../../SpectreConsole/Markdown.TextMate/Docs/Supported-Grammars/en.md). This increment requires a fresh test run.
