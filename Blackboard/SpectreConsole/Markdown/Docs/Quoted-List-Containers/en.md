# Quote-contained lists under ordinary lists — terminal presentation

The Markdown Spectre renderer composes outer ordinary-list indentation,
theme-controlled quote markers, inner quoted-list indentation and optional
code indentation. Parser and renderer keep independent responsibilities.

## Example

~~~~markdown
- Install
  > - Check the environment
  >   12) Review the runtime
  >       ```bash
  >       dotnet --list-runtimes
  >       ```
  Continue
~~~~

- The outer list marker comes from `QuoteContainerListMarker`; inner
  list owners and `ListContinuationMarker` stay independent.
- `MarkdownSpectreTheme.ListIndent` controls list depth, `QuotePrefix`
  and `QuoteMarker` style quotes, and `CodeIndent` styles code bodies.
- Plain quoted prose that is directly owned by the outer item does not
  receive an accidental second outer indent.
- Code-language labels, optional literal delimiters and TextMate-colored
  lines use the same outer-quote-inner indentation.
- Source snippets are printed safely. No Markdown content is executed
  or interpreted as terminal markup.

This is not a complete CommonMark container renderer.

[Parser design](../../../../../ExchangeHouse/Markdown/Docs/Quoted-Lists-Inside-Lists/en.md) ·
[Renderer implementation status](../../IMPLEMENTATION_STATUS.md).
