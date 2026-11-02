# Incremental Markdown scanner (experimental)

The original MarkdownParser.Parse(string) stays unchanged and returns the
full immutable MarkdownDocument. MarkdownStreamingReader.ScanAsync(TextReader)
adds a forward-only bounded-memory scanner for very large inputs.

## Index open structures instead of retaining their contents

- Keep only the active fence marker, opening marker length, language hint,
  64-bit source character offset and quote depth.
- Keep a bounded stack of list indentation candidates.
- Keep only the currently processed physical line in a bounded buffer.
- Yield a completed line immediately, including its original line ending.
- When the corresponding closing fence arrives, release the fence metadata.
  A code fence with millions of short code lines needs no giant buffer.

The maximum physical line size defaults to 64 Ki UTF-16 characters; it can
be adjusted explicitly. There is no arbitrary maximum logical code-block
size. A line that exceeds the limit fails with InvalidDataException instead
of silently truncating source. Cancellation is supported.

Source positions are long (64-bit) **UTF-16 character offsets**, not UTF-8
byte offsets. Full byte offsets require a separate encoding-aware index.

## API usage

Call MarkdownStreamingReader.ScanAsync(reader, options, cancellationToken)
with any TextReader, including a virtual generator. Each MarkdownStreamLine
has Kind, Start, RawText, Content, QuoteDepth, ListDepth, OpenFenceStart and
CodeLanguage. Consumers should render, index, store or discard each line
before requesting the next one. Avoid collecting the sequence to a list when
handling giant documents.

The Blackboard Markdown renderer offers RenderStreamingLine for these events.

## Explicit limitations

This is a conservative structural scanner, not CommonMark-complete parsing.
It does not yet guarantee full cross-line list ownership, quote nesting,
reference-style link resolution, paragraph-level inline semantics or an
editable AST. ListDepth is a bounded best-effort index. The original
MarkdownParser and MarkdownWriter remain the correctness baseline for
fully modeled small documents. Do not claim equivalent output for all MD.

For stress testing and constraints see the tool at
Toolroom/Markdown/Stress/README.md.
