# Future feature: incremental term and phrase index for Markdown navigation

**Status:** approved design direction, **not implemented**. Captured 2026-11-04 after the first disk-backed source-line pager and the end-of-document Enter/Escape session work. This design must not change current keyboard behavior until a tested visual Markdown pager exists.

## Motivation

READMEs are often in English. Users repeatedly look for familiar topics such as **Installation, Build, Compilation, Configuration, Usage, Examples, Requirements** or **Troubleshooting**, but should not have to remember exactly which sections a repository contains. They should also be able to enter arbitrary words or phrases.

A full-text inverted index of every token may cost more RAM, disk and startup time than a small terminal viewer needs. Instead, combine:
- A *small configurable seed term set* and actual parsed Markdown headings.
- Search-on-demand: when a term or phrase is not indexed, scan the validated source, store its match positions and reuse them for the remainder of that document version.
- A compact, disk-backed postings list (positions, not copied paragraphs) with bounded memory and a configured disk budget.

Suggested default **English examples**, not a mandatory or exhaustive list: `Installation`, `Build`, `Compilation`, `Configuration`, `Usage`, `Examples`, `Requirements`, `Troubleshooting`. Do not hard-code those as a language-specific constraint; future configuration should accept users' own terms and other languages, and headings can be collected independently.

## Planned interaction

- `F`: open a list of indexed topics/actual heading titles with a text entry for an arbitrary word or phrase.
- Select a term: jump to its first occurrence; show its result count and source or visual line context.
- `N`: next match; `P`: previous match. Define explicit wrap-around behavior or a "no more matches" message in the UI contract before implementing either.
- New term: one bounded source scan; cache the exact match positions; then navigate just like a built-in term.
- `Esc` should close the search overlay before exiting the document; `Enter` retains the documented menu behavior only where it does not conflict with a focused search prompt.

**No key binding above is active yet.** Current rendered `mdview` still ignores `F/N/P/PageUp/PageDown/arrows` after rendering. The `source PATH` raw pager has its own independent controls.

## Index, consistency and search semantics

Keep *two* related indexes, not a single ambiguous structure:

1. **Heading outline**: from parser-confirmed Markdown headings, with source line, source offset, level, title and a stable navigation target. It must distinguish actual headings from heading-like text inside fenced code and nested containers.
2. **Search postings**: key = normalized query + matching options; values = sorted source positions of exact occurrences. Store byte offsets or a source-line-plus-column pair; include a mapping from source positions to wrapped visual lines for later rendered paging.

Additional choices requiring explicit tests:
- **Case and language**: case-insensitive default for English; Unicode case folding and diacritic behavior must be specified (do not assume culture-sensitive string search is stable across platforms). Word boundaries and substring vs whole-word matching are options, not silent heuristics.
- **Phrases and overlaps**: support multiple-word queries, adjacent and overlapping results, CRLF boundaries and Unicode surrogate/UTF-8 offsets without corrupting navigation. No regex by default; optional regex would require complexity/time budgets.
- **Limits**: maximum query length and indexed-term count, maximum postings per term and disk use, capped in-memory buffers, cancellation, progress feedback, and explicit "too many matches" handling. Never silently truncate matches while claiming an exhaustive result.
- **Versioning**: cache identity must include verified content digest + normalisation/index format version and query options. Never reuse offsets after a document changes (a modification timestamp alone is insufficient). Remote README caches remain separately persistent and offline-usable.
- **Durability**: use the existing `VerifiedDocumentCache` identity and validation concepts or a dedicated atomic sidecar; clean or rebuild corrupt postings safely. Cached search indexes may persist across restarts, but must have a bounded eviction/cleanup policy. Do not delete verified README content when pruning optional derived indexes.
- **Timing**: index construction is asynchronous and cancellable; indexed search returns quickly after construction, but the first search on a huge file may still require a full sequential scan. Any "instant" performance promise requires a benchmark.

## Prerequisites and delivery order

1. Establish a conservative, bounded forward Markdown line/block scanner that preserves code-fence state, source positions and unmodified text without parsing the entire document at once.
2. Build a rendered *visual-line-aware* pager and mapping between source offsets and visible rows; preserve link/quote/list/fence semantics and ANSI/TextMate literal safety.
3. Add a *heading-outline prototype* using only parser-confirmed structure, not naive `#` text matching.
4. Add small seeded phrase indexes and on-demand index generation; persist safely only once validation, invalidation and disk budgets are tested.
5. Implement the `F` selection prompt and `N/P` navigation; test cross-platform keyboard behavior and accessibility.
6. Benchmark slow HDD, SSH, huge files, Unicode and multi-language README collections; optionally add user-defined seed topics.

This is a roadmap, not a claim of implemented search, immediate navigation, exhaustive CommonMark parsing or fast first-query performance.

[mdview README](../../README.md) · [Disk-backed UTF-8 lines](../../../../TextFiles/Docs/Disk-Line-Index/en.md) · [Document completion keys](../Document-End-Navigation/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
