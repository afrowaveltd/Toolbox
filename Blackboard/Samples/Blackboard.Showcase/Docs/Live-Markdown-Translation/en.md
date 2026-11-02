# Streaming translated Markdown — design proposal (not implemented)

## Desired experience

A user opens an enormous remote Markdown document and selects their preferred
language. The original HTTPS response is streamed into an ephemeral local
file. As soon as validated text becomes available, a Markdown-aware streaming
scanner emits **translation-safe semantic segments**. UnderstandMe/Afutrans
translates those asynchronously, and a page reader consumes finished target
segments while the rest is still downloading or being translated.

This is a proposal, not a claim that live translated rendering exists.

## Pipeline

1. HTTP response -> bounded binary chunks -> original temporary file.
2. Strict incremental decoding (BOM/encoding state across chunk boundaries,
   no lossy replacement of invalid bytes) -> Markdown structural scanner.
3. Scanner -> small bounded queue of translatable segments, with stable
   long-valued source offsets, sequence IDs and language context.
4. UnderstandMe adapter translates prose and supported image alt/title text.
   Code fences, inline code, URLs, paths, link destinations and raw syntax are
   not sent to the translator.
5. Finished translations -> ordered target stream and optional second
   temporary file + offset index. Preserve original source and mapping.
6. UI pager reads completed translated pages. It may wait only on the next
   required segment, never on the lifetime of the original HTTP request.
7. Dispose/cancel closes HTTP, workers, readers and temp files. Partial
   segments must not be shown as final, and incomplete translations are
   clearly marked.

## Correctness and performance constraints

- **Translation segments must be semantic units**, not arbitrary 8-KiB
  network packets or physical lines: paragraphs/list items may contain
  inline markup and links, while code fences are non-translatable.
- Preserve order even if translation calls complete out of order, with a
  bounded reordering queue and backpressure. Do not accumulate unlimited
  text in RAM while a provider is slow or offline.
- Reuse UnderstandMe's queues, priority rules and dictionaryless fallback,
  rather than inventing a second translation platform. Avoid per-line remote
  requests if batching whole short paragraphs reduces latency and cost.
- Store translation cache separately from raw local spool. Respect the
  existing policy that real-time translations must not persist to shared
  cache. Do not upload confidential documents to external providers without
  explicit provider/privacy policy.
- Translation length varies by language. Original byte offsets cannot index
  the translated file directly. Maintain a per-segment source-to-target map.
- Rendering may be paged and seekable before the entire remote response is
  complete, but the growing-file protocol needs a producer/consumer
  coordination mechanism and explicit published offsets. A partially
  flushed page must never be mistaken for completed source.
- Define handling of late Markdown reference definitions, footnotes,
  annotations, nested containers, tables and links before asserting
  complete CommonMark behavior.
- A translated view is a *derived representation*; do not modify the
  original source file. Users must be able to switch back to source text.

## Possible interfaces

Keep the translation backend behind an injected interface. Pass semantic
segments with source IDs, source language, destination language, optional
context and cancellation. Return translated content plus mapping metadata
rather than mutating raw Markdown strings. Support a local-only provider,
fallthrough/offline errors and future durable translation histories.

## Phases

First finish safe download-to-disk + line scanner and ordinary paging. Then
implement strict decoder and semantic block segmentation. Add the UnderstandMe
adapter and ordered translation results. Finally allow overlapping download,
translation and paging over a synchronized growing file.

This keeps a 10 GiB input from requiring 10 GiB of RAM and avoids holding
network connections open for the duration of human reading.
