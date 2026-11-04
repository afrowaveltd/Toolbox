# Code fences: all bundled TextMate grammars

## How it works

The optional `MarkdownTextMateHighlighter` resolves the first language identifier from a fenced Markdown code block (such as `javascript` in `\`\`\`javascript`). It uses:

1. Case-insensitive normalization of common language names, command aliases and filename suffixes.
2. A direct lookup of the resulting language ID in `TextMateSharp.Grammars`' bundled registry.
3. An extension-based registry lookup for other supported languages when the ID lookup finds no grammar.

**There is no hard-coded whitelist of languages.** New or uncommon language IDs work automatically **when the installed TextMateSharp grammar bundle contains them**. The project currently references `TextMateSharp.Grammars` **2.0.4**, which ships grammars for **50+ languages** including JavaScript, TypeScript, C#, Python, C/C++, Rust, Go, Java, PHP, Ruby, Swift, Dart, PowerShell, Bash, SQL, HTML, CSS, SCSS, Less, JSON, XML, YAML, Markdown, LaTeX and many others. Consult upstream for the version-specific grammar inventory: https://github.com/danipen/TextMateSharp

Not every language, file extension or dialect is supported by that bundle, and no colors are invented for unsupported grammars. `toml`, AJIS-native constructs and new languages without a bundled grammar require an additional grammar provider in a future increment; `ajis` currently uses the closest JSON-like highlighting as a visual approximation, not an AJIS parser.

## Examples

The following Markdown blocks are all routed through the same interface:

````markdown
```javascript
const greeting = (name) => "Hello " + name;
```

```typescript
interface User { id: number; name: string; }
```

```python
def greeting(name):
    return f"Hello {name}"
```

```json
{ "name": "Toolbox", "enabled": true }
```

```yaml
language: cs
enabled: true
```

```sql
SELECT name FROM users WHERE id = 1;
```

```rust
fn main() { println!("Hello"); }
```

```html
<strong>Hello</strong>
```

```css
strong { color: blue; }
```
````

Aliases include `js`, `node`, `mjs`, `ts`, `py`, `c#`, `cs`, `c++`, `cpp`, `rs`, `golang`, `bash`, `sh`, `zsh`, `pwsh`, `ps1`, `yml`, `jsonc`, `ndjson`, `htm`, `svg`, `diff`, `patch`, `md`, `docker` and many others. The original fence label stays visible and unchanged; only grammar selection is normalized.

## Consumers

- `Blackboard.Showcase`: its existing Classic/Ocean/Forest themed Markdown previews use the optional TextMate adapter. The gallery now includes JavaScript, JSON and YAML examples alongside C#.
- `mdview`: now explicitly references the TextMate adapter and supplies a syntax-enabled `MarkdownSpectreTheme` on **every** local, HTTPS and GitHub README render; previously its default theme omitted this adapter.
- `ExchangeHouse.Markdown` and the base `Blackboard.SpectreConsole.Markdown` renderer: intentionally **do not depend** on TextMateSharp or native libraries. Other consumers must opt in via `MarkdownSpectreTheme.SyntaxHighlighter`.

The adapter creates a per-fence stateful grammar session, preserving multi-line constructs. Theme rules produce visible code colors when a terminal supports ANSI color. It never executes code, launches a browser, downloads grammars or interprets code as Spectre markup. A missing grammar, native runtime issue, malformed language hint or tokenizer failure yields **safe literal code**; lines over 16,384 characters are displayed without highlighting and tokenization has a 40 ms/line budget.

## Verification

```bash
dotnet test Blackboard/SpectreConsole/Markdown.TextMate.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.TextMate.Tests.csproj
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd toolbox
```

Regression tests cover common aliases, selected bundled grammars for programming/data/web formats, actual JavaScript token styling, literal Spectre markup fallback and mdview theme wiring. **The new increment is committed but its tests are not yet verified** in the current execution environment; the prior user-confirmed solution baseline was 6,443/6,443 before the interactive picker and highlighter updates. Manual Windows/Debian/SSH color checks remain necessary.

[Grammar adapter](../Grammar-Adapter/en.md) · [README](../../README.md) · [Status](../../IMPLEMENTATION_STATUS.md).
