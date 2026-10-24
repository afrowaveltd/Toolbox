# Optional UnderstandMe localization adapter (design)

**Status:** proposed future integration; not implemented or published. The current priority remains verifying FileBrowser and building the UnderstandMe configurator and translation pipeline.

## Goal

Keep the base Spectre.Console FileBrowser independently usable and English-first. An optional adapter (working name **UnderstandingFileBrowser**) supplies `SpectreFileBrowserLabels` from the **host application's existing UnderstandMe localization instance/context**. Avoid a second translation runtime, duplicate language settings or mandatory dictionaries.

## Dependency direction

- Base FileBrowser must not reference UnderstandMe.
- UnderstandMe must not reference FileBrowser.
- The optional adapter references both, and maps stable semantic UI labels to the host's usual translation calls.
- Host controls culture, default language, translation providers, caching, policy and lifecycle; adapter must not create its own isolated dictionary or global localization instance.

## DictionaryLess behavior

- A missing key immediately resolves to its English source text and enters the normal application's DictionaryLess registration/translation workflow.
- Keys should be stable and namespaced (e.g. `Blackboard.FileBrowser.Action.Parent`) while preserving an English fallback value; adapter should avoid accidental key renaming and duplicate entries.
- When translations become available, use them on the **next safely refreshed UI view**. Do not interfere with an active keyboard prompt or asynchronously rewrite its menu mid-selection.
- Runtime text parameters and terminal markup must be escaped and handled safely; translation strings are data, not trusted Spectre markup.
- Cancellation, failures and offline mode must preserve a usable English interface and must not block navigation.

## Optional resources

The adapter should not ship mandatory dictionaries. Separate optional language-resource packages may offer pretranslated/offline terminology, without preventing DictionaryLess usage or imposing these resources on the base FileBrowser. Licensing and precedence of optional dictionaries require a later decision.

## Proposed acceptance criteria

- English-only FileBrowser works without UnderstandMe dependencies.
- The same host culture/provider/cache is used for all adapted labels.
- Missing translations trigger normal DictionaryLess fallback and registration exactly once per expected key according to UnderstandMe semantics.
- Language changes take effect at a safe prompt boundary.
- No navigation/input regression, no extra network dependency in the base package, and no uncontrolled terminal markup.

See [FileBrowser README](../../README.md) and the [Implementation Status](../../IMPLEMENTATION_STATUS.md).

## User-triggered translation refresh (design decision, 2026-10-24)

When the host's UnderstandMe context reports that translations for the current view are pending, the adapter may show a subdued footer such as `Translation in progress — Refresh`. Refresh is an explicit UI action (potential shortcut subject to Spectre.Console input feasibility), not a timer-driven redraw.

- Render immediately from current localized values or already-known default-dictionary values.
- On user refresh, re-resolve visible labels from the existing localization context/cache. Do not instantiate a new dictionary, translation pipeline or picker session.
- Preserve current location, active filters and selection/navigation state insofar as the underlying prompt API supports it; never mutate filesystem or start a new search as a side effect of refreshing labels.
- Leave still-missing values at the default text. A refresh must not register duplicate keys or enqueue duplicate work beyond normal UnderstandMe idempotent semantics.
- Display pending only when the host exposes a reliable pending state. Completed, failed or offline translations must not be misreported as actively in progress.
- The base FileBrowser remains independent of translation status; the optional integration owns the footer and refresh behavior.

This is a proposed UX contract, **not implemented**. Verify what Spectre.Console can preserve across a prompt refresh before promising a single-keystroke refresh.
