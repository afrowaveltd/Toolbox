# Localization — first independent implementation

**Status:** first code slice and tests committed; compiled/tested status pending verification. Independent .NET 10 UnderstandMe library, no Dita or Afutrans dependencies.

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
| `JsonTextLocalizer` | Offline core implementing the interfaces using a pluggable store. |

## Simple usage

```csharp
ITextLocalizer t = new JsonTextLocalizer(new FileJsonDictionaryStore("Locales"));
string result = t.Format("Hello {name}",
    new Dictionary<string, string> { ["name"] = "Bolaji" }, "sw");
```

With no existing locale files, this returns `Hello Bolaji` immediately and writes `{"Hello {name}":"Hello {name}"}` into `Locales/en.json`. Nothing requires a server or translation engine.

## Testing

Command: `dotnet test UnderstandMe.Tests/UnderstandMe.Tests.csproj` (or after .sln registration, `dotnet test Toolbox.sln`).

**14 xUnit tests prepared, not yet verified:** dictionaryless startup, target precedence, parent/source fallbacks, no overwrite, opt-out registration, placeholder formatting, unknown placeholders, no recursive substitution, double braces, explicit reload, malformed reload retaining valid snapshot, corrupt source preservation, unsafe path rejection, duplicate JSON keys, and concurrent registration.

## Deliberate limits / next steps

- Current catalog refresh is explicit `Reload`, not a file watcher. Automatic change detection follows after tests pass.
- Only basic named placeholders are formatted. No protected placeholder translation masking, sidecar JSON hints, iterable variants, grammar or CLDR yet.
- The file store serializes writes **within one instance**; distinct processes need coordination to avoid conflicting edits.
- Symbolic application keys (e.g. current Afutrans `App.Title`) require a supplied source dictionary or an adapter; `key=key` alone would display the symbolic key.
- Afutrans UI scaffolds are disposable except the user-authored `WebTranslator`, which must not be rewritten. Its current localizer is not replaced in this commit.
- Dita is a fully independent project and was used only as conceptual inspiration. There is no source-code migration.
- Future work: confirm build/tests, add stricter placeholder structure validation, optional sidecar metadata, adapt Afutrans, then add hot reload and optional REST/SignalR exchange.
