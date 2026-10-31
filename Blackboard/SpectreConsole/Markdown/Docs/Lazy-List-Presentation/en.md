# Lazy list paragraph presentation

The UI-neutral ExchangeHouse scanner now reports paragraph-only lazy continuations as owner-aware `MarkdownBlock` entries. The Spectre.Console renderer already aligns any paragraph with `ListContinuationMarker` beneath its displayed bullet or numbered marker; no additional renderer branch is needed.

```markdown
- First
This lazy line still belongs to First.
- Second
```

With a normal bullet theme the second line is displayed beneath First's content, without a repeated bullet. Nested numbered items retain their original marker width and use the chosen theme's `ListIndent`. Virtual-console regression tests cover both layouts.

The renderer only consumes the source model, never rewrites Markdown, resolves paths, opens links, or executes markup. For recognition boundaries and examples, see [the parser's lazy-continuation documentation](../../../../../ExchangeHouse/Markdown/Docs/Lazy-List-Continuations/en.md) and [implementation status](../../IMPLEMENTATION_STATUS.md).
