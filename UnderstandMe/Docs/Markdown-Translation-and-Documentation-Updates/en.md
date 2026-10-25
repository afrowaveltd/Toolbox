# Markdown translation, documentation references, and help delivery

**Status: architecture/design decisions, not yet implemented.** Discussed 2026-10-25. Work incrementally; confirm individual design decisions with tests before proceeding to the next slice. This document is an implementation reference, not a claim that these features are complete.

## Design philosophy and provenance

- Build in small, independently reviewable and tested increments, not a one-shot implementation. Revisit edge cases before locking APIs; document unresolved questions explicitly.
- Use `afrowaveltd/AfrowaveWebPublic` (particularly `Id` and `SharedTools/Services/MDServices`) as **inspiration**, not code to copy blindly. Keep familiar useful concepts such as tokenization, translatable versus protected segments, configurable mappings, and reconstruction, while fixing limitations.
- Prefer a self-maintained, modular Markdown parser and writer for long-term control, translation-aware behavior and optimization. Respect CommonMark conventions where practical; benchmark before optimization. Reuse an existing renderer initially for the Configurator help UI.
- Separate syntax parsing, Markdown writing, translation segment policy, document reference resolution, validation and display. Markdown translation must preserve sentence/paragraph context, not split every word or inline token into isolated requests.
- Treat English `Docs/<topic>/en.md` as source truth in the current documentation convention, with translated files generated later and optionally human-edited with audit.

## Parsing and translation pipeline

`Parse -> identify protected nodes and context-aware translatable segments -> translate -> restore/rebuild -> validate against original -> repair/review if necessary -> publish`.

- Translate visible link labels, headings, paragraphs and meaningful descriptions. Protect URL destinations, emails, commands, inline/fenced code, placeholders and other literal data.
- Use structural nodes and stable protected-element identities rather than trusting arbitrary string replacement. A link label is translatable; its destination is resolved separately.
- Writer should support lossless parse/write round trips when no edit is requested, including whitespace and line endings; preserve original source spans/trivia or equivalent information where necessary.
- Validate link identity and destination mappings, not merely raw link count. Check images, tags, code fences/blocks, placeholders, tables and structural consistency. Different natural-language sentence counts are not inherently errors.
- Structural failures must prevent automatic publication. Semantic comparison/audit of translation against the **original** should be a separate configurable stage; failures may trigger repair or human review, without assuming every stylistic difference is an error.

## Language-aware document reference resolution

Never replace `en` with `cs` solely because those characters appear in a URL. Resolve document identity and target language through a managed document registry, supporting both `Docs/WhenItFails/en.md` and `Docs/en/topic.md` layouts and optionally explicitly registered owned absolute URLs.

- Classify links: managed/local, explicitly registered owned absolute, external or unresolved. Unknown and external targets are left unchanged by default; do not fabricate translations on third-party domains.
- Relative paths must be calculated from the **translated source document's location**, not from the source language's path.
- Track targets scheduled for translation, even if their localized files do not exist yet. Check fragments/anchors using stable heading identities or validated mappings where supported.
- Default documentation export policy for managed references: **DualLinks**. Keep the localized target link even when the translated target has not yet been published, *and* retain a separately clickable English original. Example:
  - Source: `See [installation guide](../Installation/en.md).`
  - Czech: `Viz [návod k instalaci](../Installation/cs.md) ([English](../Installation/en.md)).`
- In original English documents, do not rewrite existing links merely for localization. Do not automatically add speculative localized links to unknown/external targets.
- A localized link may temporarily return 404; the original link is explicit fallback, assuming the original itself is available. Validate both URLs and report missing localized targets as expected/pending versus unexpected broken links; absence of an unpublished translation must not automatically fail a whole documentation batch.
- Link validation compares **logical references**: one managed source reference may legitimately expand into two physical Markdown links. Assert original targets remain intact, translated targets correspond to the same document identity and language, and there are no unaccounted additions.
- Alternate policies may include `LocalizedWithFallback`, `OriginalOnly`, and `LocalizedOnly`; `DualLinks` is the preferred default for Afrowave documentation. Web/TUI may present a compact language switch, but exported MD must work independently of the application.

## Configurator help and online delivery

- Add Help to the Configurator main menu and context-specific actions (e.g. Register provider, storage, validation, live connection diagnostics). Returning from Help must preserve unsaved editor state and selection context.
- Keep source help under topic-specific `Docs/.../en.md`. Embed a baseline English help set in the executable for **permanent offline fallback**; the embedded resources cannot be modified without rebuilding the executable.
- Optional online updates eventually use Afutrans Web REST API for version manifests/downloads and SignalR notifications to trigger checks; REST/startup checks must work without SignalR. Default to an explicitly configured local host/port during development; production endpoint configurable and secured.
- Maintain at most one active verified downloaded version for each needed compatibility set, not an accumulating history. Atomically replace downloaded cache; keep embedded fallback unchanged. A damaged/missing cache must never block help.
- Separately version application, documentation and document format/reader capabilities; manifest must express app compatibility, locale, document identity, integrity hash and preferably signed publishing metadata for production. Only activate compatible, validated content. Manual repair/reset is permitted.
- The settings for help updates belong to standalone Configurator preferences, **not** the edited UnderstandMe host configuration. Application updates must respect installation/distribution method (standalone, package manager, NuGet, web deployment).
- Live translation-backend probe latency is *not* representative of actual translation throughput. Later routing metrics should be measured from the executing host during real requests (e.g. rolling latency, p95, availability and load), separately from `GET /languages` connectivity diagnostics.

## Open decisions / future increments

1. Define minimal document AST, stable link/heading identity model, source trivia retention and versioned extensions.
2. Define resolver registry and reference/missing-target severity policy (including paths outside project, absolute URLs, images and anchor links).
3. Define portable DualLinks representation and validation without requiring Afutrans runtime.
4. Implement offline Configurator help with existing renderer; add tests and keep implementation status current.
5. Incrementally implement the custom parser/writer, segmenter, resolver and structural validator with round-trip, edge-case and security tests.
6. Implement Afutrans Web documentation manifest/update API, SignalR notification and compatible client updater only when that service exists; do not assert this exists now.
