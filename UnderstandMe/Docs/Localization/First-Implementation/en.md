# Localization — first independent implementation

**Status:** first code slice implemented and verified by GitHub Actions [run #39167610994](https://github.com/afrowaveltd/Toolbox/actions/runs/39167610994). .NET 10 solution build passed with zero warnings. 14/14 UnderstandMe tests and 5,928/5,928 solution tests passed. Independent .NET 10 UnderstandMe library, no Dita or Afutrans dependencies.

## Implemented public interfaces

| API | Responsibility |
| --- | --- |
| `ITextLocalizer.Get(key, culture?)` | Target locale lookup, parent locale and source fallback. Missing source text is returned unchanged. |
| `ITextLocalizer.Format(key, values, culture?)` | Named `{name}` placeholders; missing names remain as tokens; inserted values are not parsed recursively. |
| `ITextLocalizer.Reload(culture)` | Explicitly refresh one cached locale from storage; invalid new file leaves previous valid snapshot intact. |
| `ILocalizationDictionaryStore.Load(culture)` | Read a flat key/string locale dictionary. |
| `ILocalizationDictionaryStore.TryAdd(culture, key, value)` | Register a new source entry if absent, never overwriting it. |
| `FileJsonDictionaryStore` | UTF-8 locale JSON files; rejects invalid flat shape/duplicates; writes with a temporary file and replacement. |
| `LocalizationOptions` | `SourceCulture` defaults to `en`; `RegisterMissingSourceKeys` defaults to `true`. |
| `DictionaryTextLocalizer` | Offline core implementing the interfaces using a pluggable store. |

## Simple usage

```csharp
ITextLocalizer t = new DictionaryTextLocalizer(new FileJsonDictionaryStore("Locales"));
string result = t.Format("Hello {name}",
    new Dictionary<string, string> { ["name"] = "Bolaji" }, "sw");
```

With no existing locale files, this returns `Hello Bolaji` immediately and writes `{"Hello {name}":"Hello {name}"}` into `Locales/en.json`. Nothing requires a server or translation engine.

## Testing

Command: `dotnet test UnderstandMe.Tests/UnderstandMe.Tests.csproj` (or after .sln registration, `dotnet test Toolbox.sln`).

**All 15 xUnit tests passed in GitHub Actions, including the independent in-memory backend test:** dictionaryless startup, target precedence, parent/source fallbacks, no overwrite, opt-out registration, placeholder formatting, unknown placeholders, no recursive substitution, double braces, explicit reload, malformed reload retaining valid snapshot, corrupt source preservation, unsafe path rejection, duplicate JSON keys, and concurrent registration.

## Deliberate limits / next steps

- Current catalog refresh is explicit `Reload`, not a file watcher. Automatic change detection follows after tests pass.
- Only basic named placeholders are formatted. No protected placeholder translation masking, sidecar JSON hints, iterable variants, grammar or CLDR yet.
- The file store serializes writes **within one instance**; distinct processes need coordination to avoid conflicting edits.
- Symbolic application keys (e.g. current Afutrans `App.Title`) require a supplied source dictionary or an adapter; `key=key` alone would display the symbolic key.
- Afutrans UI scaffolds are disposable except the user-authored `WebTranslator`, which must not be rewritten. Its current localizer is not replaced in this commit.
- Dita is a fully independent project and was used only as conceptual inspiration. There is no source-code migration.
- Future work: confirm build/tests, add stricter placeholder structure validation, optional sidecar metadata, adapt Afutrans, then add hot reload and optional REST/SignalR exchange.

## Backend-independent dictionary sources (decision 2026-10-19)

UnderstandMe is **not inherently a JSON localization library**. Its public localizer uses the `ILocalizationDictionaryStore` contract; `FileJsonDictionaryStore` is just one adapter. The now-neutral `DictionaryTextLocalizer` name replaces the misleading pre-release `JsonTextLocalizer` name.

Potential adapters: flat JSON files (as Afutrans prefers), in-memory data, SQL/other databases, remote REST/SignalR endpoints, Node.js services or other remote nodes. The logical catalog is a normalized string key/value snapshot, not a requirement for a particular persistence or wire format.

Additional capabilities should be **optional and interface-based**: read, write, change notification and metadata hints. Read-only sources are valid. Remote/database refresh must use async APIs and cancellation instead of blocking synchronous UI lookup on network I/O; a fast cached `ITextLocalizer.Get` reads the current valid snapshot. A source revision/change notification can trigger background refresh, and absent connectivity preserves offline fallback. Future async interface names and signatures are deliberately not final until an actual second backend is implemented.

Do not confuse the format used to exchange dictionaries (often REST/JSON) with the host's storage technology. The flat JSON format remains the Afutrans interoperable convention; UnderstandMe may use other backing stores. Dictionaryless registration requires a writable source or a separately configured deferred registration target.

A new `MemoryStore` xUnit test proves that lookup, first-key registration and explicit refresh function without any JSON file, using only the interface. No remote, SQL or Node.js adapter has yet been implemented.

## Named placeholder candidate validation (2026-10-20)

**Small additive implementation slice:** `LocalizationPlaceholderValidator.HasMatchingPlaceholders(source, candidate)` provides a strict, opt-in check for proposed translated templates. It requires the **same ordinal placeholder names with the same occurrence counts**, including duplicate slots; order may change for target-language grammar. `{{` and `}}` are treated as escaped literal braces. Stray/unmatched braces, empty/invalid names, removed/added/renamed slots are rejected.

This validator does **not** require a dictionary, CLDR, provider, Afutrans or metadata. It does not alter `ITextLocalizer`, `DictionaryTextLocalizer.Get/Format` or dictionaryless source registration. In particular it is **not yet integrated into translation admission/import or the existing localizer's legacy translations**; consumers can use it before accepting a candidate. Future integration must handle a previously stored translation with extra slots consciously, without silently breaking source registration.

Tests in `UnderstandMe.Tests/Localization/LocalizationPlaceholderValidatorTests.cs` cover reordering, repeated placeholders, escaped braces, Unicode plain text, case sensitivity, invalid tokens/braces and null inputs. Build/CI results belong in `IMPLEMENTATION_STATUS.md` once observed. **No additional grammar, hint sidecars or CLDR engine implemented.**

## CI verification of named placeholder validation (2026-10-20)

The additive validator commit [`f22ec3d`](https://github.com/afrowaveltd/Toolbox/commit/f22ec3d3de9e14ee7e0b2167283235dc75c243d6) passed the .NET 10 workflow [run #39407132671](https://github.com/afrowaveltd/Toolbox/actions/runs/39407132671): zero warnings/errors, **43/43 UnderstandMe tests and 5,957/5,957 full solution tests**. The validator remains opt-in; no automatic activation in localizer lookups or future translation admission yet.

## Safe runtime fallback for mismatched localized placeholders (2026-10-20)

`DictionaryTextLocalizer` now applies `LocalizationPlaceholderValidator.HasMatchingPlaceholders` to each stored target candidate by default **at lookup time**. Source template is the authoritative source-locale dictionary **value** for symbolic keys, otherwise the dictionaryless source key. If the selected target adds, removes, renames or duplicates a placeholder (or has malformed braces), `Get` tries a valid parent locale then valid source template before returning key. `Format` substitutes values into this safe fallback, avoiding silently lost information. Locale files are not modified or deleted; offline dictionaryless start remains intact. A legacy host can explicitly opt out with `LocalizationOptions.ValidateLocalizedPlaceholders=false`.

Unit tests were added for missing/extra/renamed/invalid slots, region-parent fallback, symbolic-source templates, valid reordering and explicit legacy opt-out. Historical test `Named_placeholders_do_not_modify_catalog` now uses a legitimate two-slot source and target instead of implicitly approving a newly introduced `{city}`.

Optional realtime rescue by translating **fully rendered source text**, scoped invalidation/change events, Afutrans regeneration, incident logging and per-entry last-good revision retention remain **planned, not implemented**, preserving synchronous cache-first lookups and source-key history. **Do not delete a dictionaryless source key to trigger translation**: it would destroy reusable translations and manual edits. Full design: [Invalid Placeholder Fallback](../Invalid-Placeholder-Fallback/en.md).

**Verified:** GitHub Actions [.NET workflow #39408931347](https://github.com/afrowaveltd/Toolbox/actions/runs/39408931347) for code commit [`b2faa1c`](https://github.com/afrowaveltd/Toolbox/commit/b2faa1cb5515f80f2c2e0448e985f2f773f4322f) completed **SUCCESS** (restore, build and test steps all green). No exact per-project test totals or zero-warning claims were extracted from the run.

## InvalidateTranslation: focused target-only repair hook (2026-10-20)

**New .NET 10 source slice committed:** [`1a08085`](https://github.com/afrowaveltd/Toolbox/commit/1a08085fc5619e03938fb76afd25c900e9717fe9) introduces `ITextLocalizer.InvalidateTranslation(culture, key)`, optional storage capability `ILocalizationTranslationInvalidationStore.TryRemove(culture, key, expectedValue)`, and `FileJsonDictionaryStore` target-only conditional deletion with temp-file JSON replacement.

When strict placeholder validation fails, the runtime now **automatically invalidates just the still-matching malformed target** by default on writable compatible stores; operators may disable automatic invalidation with `AutoInvalidateInvalidTargetTranslations=false`. Malformed source syntax and source-culture entries are protected; stale snapshots are refreshed after conditional delete or compare failure. Existing offline source/parent fallback preserves the caller's interpolated placeholders. Read-only custom stores still work without any delete capability.

Updated xUnit tests for explicit invalidation, missing/repeated/renamed tokens, target-only and source preservation, invalid-source opt-out, opt-in/out, CAS safety, malformed JSON and optional read-only stores. The **Afutrans scanner able to notice missing targets even with unchanged source snapshots** is designed but not yet implemented, nor are event broadcasting, incidents or realtime one-shot rescue.

**Verification:** GitHub Actions build/tests for this code commit must be observed separately; no local `dotnet` SDK is available in this tool runtime. Do not claim additional passing test totals without checking CI.
