# Code fences inside explicit blockquotes

This source-preserving parser extension recognizes backtick and tilde fences
inside **explicitly quote-prefixed lines**, including a quoted-list child.
It does not implement complete CommonMark block-container nesting.

## Example

~~~~markdown
> - Examples
>   12) C# snippet
>       ```csharp
>       if (args.Length > 0)
>       {
>           Console.WriteLine(args[0]);
>       }
>       ```
>   13) Continue with Markdown
~~~~

The opener, code lines and closer belong to the matching quoted-list
`EmbeddedBlocks` when the content indentation is sufficient. They retain
`QuoteDepth`, `CodeFenceStart`, `CodeFenceInfo`, `CodeLanguage`,
`IsClosingCodeFence` and absolute source positions. `ContentBlocks`
retains attachment order; `MarkdownWriter.Write` reconstructs source unchanged.

Code text is literal: Markdown-looking list markers, headings, quotations,
nested-looking fences and Spectre markup receive no Markdown inline tokens.
An explicit quote prefix at the opener's depth is required on **every** body
line, including blank lines. A missing or different prefix interrupts the
fence before that line, which is then classified normally.

A closing delimiter must match the original backtick/tilde marker and have
at least its opening count, with no nonwhitespace trailing characters.

This bounded grammar does not support lazy unmarked quote continuation,
arbitrary mixed quote/list AST containers or full CommonMark semantics.

[Quoted list ownership](../Quoted-Lists/en.md) ·
[Parser status](../../IMPLEMENTATION_STATUS.md).
