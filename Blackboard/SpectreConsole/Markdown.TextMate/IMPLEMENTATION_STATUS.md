# Markdown.TextMate — Implementation status

## 2026-10-31 — optional grammar adapter

- Independent .NET 10 source package with TextMateSharp.Grammars 2.0.4.
- Alias normalization, per-fence stateful tokenizer, safe literal rendering and fallback.
- Includes tests. CI pending at implementation commit; native assets need cross-platform confirmation.
- Not publicly published.

### Cross-platform green verification — 2026-10-31

- [GitHub Actions #41251065292](https://github.com/afrowaveltd/Toolbox/actions/runs/41251065292) **SUCCESS** for source commit `5b80ad1e3f9cde50ae793da53b3b81c30d727d08`. Linux complete solution restore/build/tests passed, Windows targeted UI and grammar tests passed, and Markdown link validation passed.
- Windows: **ExchangeHouse Markdown 66/66**, **Blackboard Markdown 24/24**, **TextMate adapter 6/6**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- The documentation link audit examined **280 Markdown files and 1,423 local targets** with no missing targets. A later docs-only commit clarified a nested-fence illustration; final docs CI is tracked separately.
- Real TextMate C# tokenization was exercised under Linux and Windows CI, including platform-specific Oniguruma dependencies. This does **not** establish manual SSH/tmux or Debian terminal acceptance, complete CommonMark support, NuGet publication, or a completed third-party transitive-license review.

## 2026-11-04 — full bundled grammar resolution and expanded language aliases

- `MarkdownTextMateHighlighter.CreateSession` now first uses a canonical language ID and then tries `RegistryOptions.GetLanguageByExtension("." + id)`, exposing all grammars supported by the installed `TextMateSharp.Grammars 2.0.4` package rather than a static C#-centric whitelist.
- Expanded README-fence aliases for JavaScript/TypeScript, C#/F#/VB, C/C++, Go/Rust/Python/Ruby, shell/PowerShell, SQL, HTML/XML/CSS, JSON/YAML, Markdown/LaTeX, Dockerfile and others. Unmapped IDs still query the registry directly. `ajis` uses JSON-like visual highlighting as a compatibility approximation, not a real AJIS parser.
- `mdview` now explicitly wires this optional highlighter into its render theme for local, HTTPS and GitHub markdown; the base ExchangeHouse parser and Blackboard Markdown renderer remain independent of TextMate native dependencies. Showcase's existing optional adapter also gains the new mapping.
- Expanded `MarkdownTextMateHighlighterTests` to check aliases, bundled programming/data/web grammars, JavaScript token colors and safe fallback. See [Supported Grammars](Docs/Supported-Grammars/en.md).
- **New source/tests not yet verified by compiler or CI** in this environment (no .NET SDK). User-confirmed baseline of 6,443 green tests was earlier than both the interactive picker and this feature. Run `dotnet test Toolbox.sln` and manually compare JavaScript/JSON/C# colors on Windows/Debian. Unsupported bundled grammar IDs render plain text rather than failing.
