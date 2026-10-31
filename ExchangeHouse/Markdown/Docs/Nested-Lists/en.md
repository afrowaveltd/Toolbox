# Nested and ordered Markdown lists

**Initial implementation:** 2026-10-31. This is a deliberately limited, source-preserving line-level list model, not a full CommonMark document AST.

Each list-item `MarkdownBlock` now carries optional `MarkdownListMarker` data: exact `OriginalMarker`, `IsOrdered`, optional `Number`, `Delimiter`, physical `IndentColumns`, calculated `Depth` and absolute `ContentStart`. `MarkdownDocument.ListRoots` exposes immutable `MarkdownListItemNode` trees; each node references its original block and exposes children. The document's existing flat `Blocks` collection and unchanged-source `MarkdownWriter` remain fully supported.

Ordered items recognize 1–9 ASCII digits followed by `.` or `)`; unordered markers recognize `-`, `+` and `*`. Adjacent items nest when indentation increases; equal indentation creates siblings, dedentation returns to an ancestor. Spaces and tab stops (four columns) are tracked without normalizing the source. A blank, paragraph, heading or fenced-code line ends a list run. Top-level roots can start with at most three columns of indentation; a continuation may be more indented. Source positions for inline links/code still refer to original character offsets.

## Non-goals and limits

- Items currently occupy a **single physical line** each. This milestone does not group continuation paragraphs, lazy continuation, loose lists or nested fenced code blocks inside list items.
- Nesting is based on relative indentation in contiguous item lines, not CommonMark content-indentation/flanking rules. Mixed ordered and unordered markers remain distinct metadata; a full typed list-container AST is future work.
- Maximum supported hierarchy depth is 32 and prefix indentation 256 columns to bound pathological input. Anything beyond supported semantics stays ordinary text.
- Exact input including CRLF/CR/LF, tabs and original numbering stays unchanged in the writer. Consumers should not renumber source documents as a presentation side effect.

The Spectre adapter will present actual numbers for ordered items and indented bullets for nested items. Source model remains UI-independent.

## Tests

Round trips with nested mixed lists, ordered delimiters/starting numbers, tab stops, code protection, interrupted list runs, unsupported syntax and exact inline offsets. Both CI and manual terminal checks are recorded in the projects' implementation status.
