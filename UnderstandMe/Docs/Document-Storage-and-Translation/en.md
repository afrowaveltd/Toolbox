# Pluggable multilingual article/document storage

Status: **design proposal, 2026-10-19; not implemented or tested**.

## Responsibility boundary

UnderstandMe translates and validates source text/documents; it must not mandate how the consuming application persists documents, metadata or indexes. Unlike application UI dictionaries, article bodies and translated titles are **document resources**, and they must never be automatically registered as globally shared `KEY=VALUE` phrases. Afutrans is a future host, not an architectural dependency.

**Candidate replaceable host-side contracts (names and signatures not final):**

- `IArticleContentStore`: read/write staged source and translated bodies identified by opaque ArticleId, culture, format and source/target revisions.
- `IArticleMetadataStore`: multilingual article titles, source culture, tags, author, publication date, status, permissions and per-language translation provenance/revisions.
- `IArticleQuery`: optional listing/searching/sorting/pagination by metadata/title/status/date, backed by SQL indexes, database search, filesystem manifest or external search service.
- `IArticlePublisher`: optional coherent publication of validated content and metadata, using the transaction/atomic-swap/versioned-manifest abilities of its provider.

One provider may implement multiple capabilities; the content and metadata stores can be physically one database, separate SQL tables, a filesystem plus metadata JSON/AJIS, MongoDB, future indexed AjisFS or a hybrid. A host selects adapters via DI/typed configuration/custom configuration, while the actual translation worker receives only normalized content, source revision, language and translation context.

## Equivalent physical examples

File-backed article ID 2OESEDR:

```text
articles/2OESEDR/
    en.md
    cs.md
    metadata.json
```

An illustrative file metadata resource (JSON is not mandatory):

```json
{
  "id": "2OESEDR",
  "sourceCulture": "en",
  "sourceRevision": "r7",
  "publishedAtUtc": "2026-10-19T12:00:00Z",
  "name": {
    "en": "Most important news",
    "cs": "Nejdůležitější zprávy"
  }
}
```

In a SQL provider the normalized data can instead be `(ArticleId, Culture, SourceRevision, Body, Status)` rows plus metadata columns/tables. A MongoDB provider could store document records with localized content and metadata. An AjisFS provider could use future indexed objects. **Do not hardcode either shape in UnderstandMe's core contracts.** For large text an object store/filesystem with a database index can also be an appropriate hybrid.

## Revision, publication, indexing and safety

- ArticleId is a stable opaque document identity; do not confuse it with the text-based UI localization key or trust it directly as a filesystem path. File adapters must validate mapped paths.
- Store source revision separately. A source edit invalidates target versions translated from an older revision. An asynchronous result must never silently overwrite newer human-approved edits.
- Publishing a new title and body requires coherence. SQL adapters may use a transaction; file adapters may write to temporary/versioned paths and atomically switch an active manifest; object stores may use conditional generation writes. Do not expose a partially committed article revision.
- A metadata/query provider handles sorting by publish date and searching localized titles; a file-body provider need not implement full-text indexes.
- Preserve privacy, licenses and tenant permissions. Translation of articles is not authorization for cross-project UI dictionary reuse.
- No assumptions that a recent MongoDB release is faster for a given workload; compare specific versions/indexes and queries with measured benchmarks. AjisFS native indexing is planned, not yet existing.

## Proposed common contract tests

1. A file+metadata JSON adapter and in-memory adapter return equivalent ArticleId/culture/title/body/revision resources.
2. SQL, MongoDB, AJIS/AjisFS or hybrid providers, if later added, must pass equivalent semantic tests for capabilities they advertise.
3. Source r8 arriving during translation of r7 prevents stale r7 publication; later human approved corrections cannot be overwritten.
4. Interrupted body+metadata publication does not surface inconsistent versions.
5. A news host can offer available per-language article versions, identify unavailable/stale versions, filter by publication date and find localized titles without changing translation-core code.

See [Architecture](../Architecture/en.md), [Afutrans Integration](../Afutrans-Integration/en.md) and [Shared dictionary indexing](../Localization/Shared-Dictionary-Indexing-and-Casing/en.md).
