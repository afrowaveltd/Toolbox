# Markdown.TextMate.Tests — Implementation status

## 2026-10-31

- Tests cover aliases, real C# grammar tokenization and safe text fallback.
- CI pending; not a package for publication.

### Cross-platform green verification — 2026-10-31

- [GitHub Actions #41251065292](https://github.com/afrowaveltd/Toolbox/actions/runs/41251065292) **SUCCESS** for source commit `5b80ad1e3f9cde50ae793da53b3b81c30d727d08`. Linux complete solution restore/build/tests passed, Windows targeted UI and grammar tests passed, and Markdown link validation passed.
- Windows: **ExchangeHouse Markdown 66/66**, **Blackboard Markdown 24/24**, **TextMate adapter 6/6**, **Showcase 26/26**, **FileBrowser 160/160**, **Configurator 44/44**, **UnderstandMe localization 34/34**.
- The documentation link audit examined **280 Markdown files and 1,423 local targets** with no missing targets. A later docs-only commit clarified a nested-fence illustration; final docs CI is tracked separately.
- Real TextMate C# tokenization was exercised under Linux and Windows CI, including platform-specific Oniguruma dependencies. This does **not** establish manual SSH/tmux or Debian terminal acceptance, complete CommonMark support, NuGet publication, or a completed third-party transitive-license review.

## 2026-11-04 — language-coverage regressions

- Added 20 common language-alias theory cases and 15 bundled grammar selection theory cases (JavaScript, TypeScript, Python, JSON, YAML, HTML, CSS, SQL, Go, Rust, Java, C++, PowerShell, Markdown). Added dedicated JavaScript-token and unknown-grammar safe-literal assertions.
- Existing C# token-color and unsafe Spectre markup regressions remain. These tests specifically exercise the installed TextMateSharp.Grammars 2.0.4 registry, not imagined syntax coloring for every programming language in existence.
- **Execution pending.** Original six TextMate tests passed in the 6,443-green user Windows baseline; the new cases were added afterwards.
