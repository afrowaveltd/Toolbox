# Rendering fenced code within quotes and quoted lists

The renderer consumes quote depth, list-owner and code-fence metadata; it
does **not** execute code or interpret source as Spectre markup.

## Example

~~~~markdown
> 12) Example
>     ```csharp
>     public static void Main(string[] args)
>     {
>         Console.WriteLine("[red]literal[/]");
>     }
>     ```
~~~~

- `MarkdownSpectreTheme.QuotePrefix` / `QuoteMarker` render quote levels
  outside the list/code indentation.
- `ShowCodeLanguage` displays a language label for an opening fence;
  `ShowCodeFenceDelimiters` prints actual delimiters with themed indentation.
- `SyntaxHighlighter` can create an optional session per code fence (including
  the separate TextMate adapter). Sessions are disposed at the closing fence
  or when a quoted fence is interrupted by an out-of-container line.
- Unknown languages, invalid spans and provider exceptions fall back to
  safe literal text. Markup-looking code never runs as terminal instructions.

This is an intentionally limited Markdown container implementation.

[Parser behavior](../../../../../ExchangeHouse/Markdown/Docs/Quoted-Code-Fences/en.md) ·
[Renderer status](../../IMPLEMENTATION_STATUS.md).
