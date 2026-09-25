# Previous-context publication selection

Status: **pre-1.0 internal recovery improvement; five focused tests awaiting local verification**.

## No new write on previous-context recovery

When flexible initialization cannot activate a new project catalog and an
earlier context exists, the default runtime can retain that context. This
path does **not** call `Set` or `Publish`; it must not claim ownership of
a newly written publication.

When its configured store implements the optional
`IErrorCatalogContextPublicationReader`, the default runtime now reads
`GetCurrentPublication()` **once** at the previous-context selection
boundary. It selects both the context and the immutable
`ErrorCatalogContextPublication` record from that single result. The
record is carried in an **internal-only**
`ErrorCatalogInitializationPayload.SelectedPublication` field and used by
`RecordStatus`. It is distinct from `OwnedPublication`, which identifies
a new write made by the default initializer, fallback or explicit reset.

`GetCompletedActivation()` still checks whether the selected record is
current when the completed observation is read. If another writer has
replaced it — including by publishing the **same context object** at a
new generation — the read returns
`WIF_ACTIVATION_PUBLICATION_CHANGED` rather than falsely assigning
the new writer's generation to the recovery status. A previously
returned status observation identifies its selected record but is
**not** a lock on the current store.

If a replacement occurs after selection, the recovery response can
contain the earlier selected context even though a newer context has
become current. This is an unavoidable consequence of independently
writing actors without store-wide ownership; the recovery response is
not a synchronized assertion that its context is still the latest.
Consumers needing current identity should use the optional completed
activation reader and handle a changed-publication response.

## Legacy compatibility

An `IErrorCatalogContextStore` without the optional publication
reader still uses the existing `GetCurrent()`-based recovery flow.
If a custom optional publication reader is unavailable, fails or
throws an ordinary exception, the runtime falls back to that legacy
path rather than changing the normal recovery outcome. Such a
fallback has **no exact selected-publication token** and its status
observation retains the previous best-effort reference association;
a failing optional reader may also make
`GetCompletedActivation()` return a structured read failure.

The selected record field is internal infrastructure data, not a new
public initialization-payload property or JSON field. The nine-method
`IErrorCatalogRuntime`, legacy store/initializer contracts, published
0.1.0 package and persisted catalog JSON schemas are unchanged.

## Verification

`PreviousContextPublicationSelectionContractTests` contains five
focused cases: a successful previous-context selection with unchanged
generation and incremented status sequence; a later same-reference
republish; a later different-context republish; legacy store behavior;
and fallback from a failing optional reader. No timing-dependent
thread sleeps are used.

The next ownership boundary is a genuinely coherent combined
catalog-and-status read, including what it means when external writers
replace a selected record after observation. A selected immutable
publication record does **not** make the source catalog's nested
objects immutable or provide a transaction against concurrent
in-place mutation.
