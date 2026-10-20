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

