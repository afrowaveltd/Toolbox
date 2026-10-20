# Invalid Localized Placeholders: Safe Fallback and Optional Self-Repair

**State: first safe offline fallback integrated 2026-10-20; all online translation, notifier, scanner and incident delivery behaviors below are FUTURE DESIGN.** This document covers the user's request for handling source/target placeholder mismatches without hiding meaningful runtime values.

## Runtime invariant and implemented scope

The existing `LocalizationPlaceholderValidator.HasMatchingPlaceholders(source, candidate)` checks brace syntax and exact **ordinal placeholder-name occurrence multisets** (duplicates matter). Reordering is allowed; adding, dropping, renaming or duplicating a placeholder is invalid. The synchronous `DictionaryTextLocalizer.Get` now uses that validator by default before returning any localized target entry, including parent-culture entries. Validation is per **source template**, not merely source key: symbolic dictionary `Key=Kids, en[Kids]=I have {count} kids` must compare against the source **value**; dictionaryless key=value uses the original source key when missing.

**Actual implemented behavior:** choose requested target if valid; otherwise try parent locale (e.g., `cs-CZ` -> `cs`) if valid; otherwise use source-culture dictionary template when valid; otherwise return the original key. `Format` then inserts the caller's values into that selected template without recursively processing the replacement values. No invalid localized candidate is returned in the default strict mode. Unrelated catalog entries remain usable.

The operator can explicitly opt out for known legacy catalogs with `LocalizationOptions.ValidateLocalizedPlaceholders = false`; the default is **true**, prioritizing source-information preservation. The dictionaryless source `TryAdd` policy remains independent. **As of commit `1a08085`, a writable target JSON entry can be conditionally removed when structurally invalid; source entries are never deleted.** This is an offline storage change only, and it does not trigger a network translation.

Examples:

| Source template | Candidate translation | Strict localizer outcome |
| --- | --- | --- |
| `I have {count} kids` | `Mám děti` | Reject; `I have 3 kids` for count=3 |
| `I have {count} kids` | `Mám {count} a {count} dětí` | Reject; extra occurrence |
| `I have {count} kids` | `Mám {number} dětí` | Reject; renamed placeholder |
| `{name} has {count} items` | `{count} položek má {name}` | Accept reordered placeholders |
| `Hello` | `Ahoj` | Accept, no placeholders |

## Optional online rescue with already-rendered text — proposed, not built

The user proposes a **one-shot realtime translation of the fully substituted source sentence** when stored locale value is structurally invalid. For example, `I have {count} kids` and `{ count: "3" }` produce `I have 3 kids`, which can be sent to an operator-permitted online translator; any successful translated result applies **only to that exact invocation** and must not be stored under the reusable template key as if it still contained `{count}`.

Important runtime split: `ITextLocalizer.Get/Format` are synchronous **offline/cache-first** APIs and must never wait for a network service. An **optional, separate async/host-controlled live rescue** may supply an updated UI result after returning immediate source fallback; a host can opt into an explicitly async API if immediate machine translation is desired. It must set a deadline, respect target privacy and provider policy, quota/budget, cancellation, and must not leak personal substituted values to unapproved third parties. If unavailable/failing, keep the safe local result and incident diagnostic. P0 RealTime stays volatile/automatic-only; no implicit durable or Human fallback for private chat.

## Repair without deleting the authoritative source phrase — proposed, not built

**The initial suggestion to physically delete source key and value to force another translation is NOT the safe default.** This would destroy dictionaryless reusable source phrases, unrelated already-valid locale translations, revision history and manual corrections. An invalid **target-locale entry** is now removed only from its own target dictionary through conditional `InvalidateTranslation(culture,key)` when writable. No source key is touched. An optional sidecar/audit record may preserve invalid revisions for incident review in a future implementation; it is not required for the missing-key scanner to discover the repair.

When notifications are configured, publish a **translation-invalidated event after a durable invalidation state is recorded** (or a host-owned diagnostic/request-for-repair event), scoped to target ID, source key identity, culture, source revision, bad target revision and placeholder validation reason. Afutrans can enqueue **just the affected locale variant** for regeneration, with idempotent event + scan reconciliation; validate the replacement template before atomic publication. No unconditional delete/recreate of all locale entries, recursive source deletion or invalidation storm. If there is a valid parent-language fallback, use it until repair succeeds.

If change events are disabled, report a **bounded/deduplicated** incident via the host's optional logger/diagnostics/WhenItFails adapter when configured; the localizer remains fully usable with safe fallback. An optional independently enabled target scanner may detect mismatches by revision/placeholder validation even with notifications off. Avoid writing network logs or side effects on each UI lookup; per-key/source-version/culture deduplication and policies prevent error floods. Incident transport, exception/Result mapping and event interface names remain design work, not implemented contracts.

## No-missing-data safeguards

- Before accepting a translated template, validate placeholder name, multiplicity, malformed braces and configured grammar/metadata constraints as applicable. A structurally valid placeholder match is **necessary but not sufficient** to assert semantic translation correctness.
- For **localized runtime values**, only the caller may decide whether to send fully rendered personal/secret data to an external processor; default remains offline source fallback. Live rendering must not be cached/shared as a generic dictionary template.
- Distinguish invalid translation from a missing **caller-supplied value**: `Format` still leaves an unmatched required token visible rather than silently erasing it; tighter caller-value diagnostics are a separate future configurable policy.
- A fresh invalid target revision must not discard the last known-good, validated translation cache if an implementation later supports per-entry revision recovery; maintain metadata to distinguish which valid revision may safely be returned. Current `Reload` keeps snapshots for invalid JSON, but **does not yet implement per-entry last-good revision quarantine**.
- For a malformed source template, the only universally safe fallback is the untouched caller key/text; automatic structural repair may require human approval. Placeholder validation follows exact names, not count-only normalization.

## Future acceptance tests

1. Source expects one `{count}` and localized value has zero or two; `Format` shows the fully rendered **source** values, never a meaningless target phrase.
2. A target has matching placeholder count but renamed identity: reject. A different ordering with exact names/counts: accept.
3. `cs-CZ` invalid and `cs` valid: select parent. Source symbolic key `Kids` uses `en[Kids]` as source template for validation and fallback.
4. Event adapter enabled: invalid target produces **one** durable-scoped invalidation/repair request, no deletion of unrelated locales or reusable source key; successful reviewed/retranslated repair has matching placeholder structure.
5. Event adapter off: safe fallback and optional deduplicated incident only; no external engine dependency, no destructive local catalog mutation.
6. Host-enabled async rendered-text translation respects privacy, request timeouts, provider budgets, cancellation and does not publish the one-shot full-value result as reusable template.
7. Offline, failure, malformed JSON or temporary online service unavailability still yields usable data without blocking synchronous UI.

**Current implementation boundary:** synchronous target-template validation, safe fallback, explicit `InvalidateTranslation` and default conditional invalid-target deletion are implemented and unit-tested. No automatic incident publisher, online realtime retry, change-event transport, quarantine store or Afutrans regeneration scanner is implemented. The implementation passed [GitHub Actions #39410576675](https://github.com/afrowaveltd/Toolbox/actions/runs/39410576675).

## Focused target-only deletion through InvalidateTranslation (2026-10-20)

**Implemented in code commit [`1a08085`](https://github.com/afrowaveltd/Toolbox/commit/1a08085fc5619e03938fb76afd25c900e9717fe9), CI verification pending at authoring time.** This supersedes the earlier suggestion of touching the source dictionary to cause regeneration, and also supersedes a default mandatory quarantine record: **remove only the invalid target-culture key/value**, leaving the source, other locales, and other keys unchanged. The future Afutrans target-language scanner can detect the **missing target** and enqueue a replacement for that exact culture.

- `ITextLocalizer.InvalidateTranslation(string culture, string key)` explicitly asks the store to delete one target entry and returns true only if it was removed. A request naming the configured **source culture is refused**; source/dictionaryless phrases must never be invalidated through this API.
- `ILocalizationTranslationInvalidationStore.TryRemove(culture, key, expectedValue)` is an **optional extension**; read-only/custom dictionary stores still implement the original `ILocalizationDictionaryStore` without change. `FileJsonDictionaryStore` implements the optional extension and rewrites the **target locale file only** using its existing temporary-file/replace approach.
- When strict placeholder validation rejects a target candidate, `DictionaryTextLocalizer.Get` automatically attempts **compare-and-remove** of precisely the still-invalid value if the store supports that capability. This is enabled by default with `LocalizationOptions.AutoInvalidateInvalidTargetTranslations=true`, and may be disabled for audit/review/read-only workflows. It is never attempted when the **source syntax itself is malformed**. Matching placeholders can still be semantically mistranslated; semantic decisions do not trigger automatic deletion.
- Conditional expected-value removal prevents **this store instance** from deleting a newer correction found at the time of removal. After a removal or conditional mismatch, the localizer refreshes that locale's snapshot so it cannot keep returning a deleted target from its own cache. Existing `Get/Format` fallback still returns a valid parent locale or source text and preserves actual placeholder values.
- This does not create a new network dependency, event bus, incident service or translator engine. A store without delete capability keeps the safe fallback without mutating its dictionary. Storage failures or invalid JSON do not cause a destructive rewrite.

**Scanner requirement (not yet implemented):** the periodic Afutrans translation-target scanner must detect **source key present / selected target key missing** even if the *source* catalog's snapshot/revision did not change. Compare per-target-locale completeness against the authoritative source, not only source-delta events; submit only missing/invalid locale work through the usual idempotent QueueMaster admission and never remove other languages. Push change notifications may accelerate scanning, but remain optional. Avoid a destructive delete/recreate loop when the backend repeatedly produces invalid placeholders; use per-key backoff/diagnostics once the scanner exists.

**Current limitations:** the file adapter serializes writes *per store instance*; cross-process file writers need external locking/versioned compare-and-set before using automatic invalidation across nodes. History/incident audit, explicit asynchronous rendered-value realtime rescue, event publishing, Afutrans scan scheduling and source/target revision-aware durable repair are still future capabilities. The existing synchronous localizer must not silently perform network translation.

**Tests added to the code commit** cover explicit single-target removal, source protection, malformed targets with zero/extra/renamed placeholders, parent-language fallback, strict opt-out, malformed source protection, comparison against a changed value, missing entry idempotence, malformed JSON preservation and an adapter without invalidation support. Run `dotnet test UnderstandMe.Tests/UnderstandMe.Tests.csproj` and the full Toolbox suite in GitHub Actions; record final observed CI result separately.
