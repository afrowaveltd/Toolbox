# Optional language catalog: endonyms, writing direction and territory filtering

Status: proposed architecture (2026-10-19). Source data are currently vendored in Afutrans, **not implemented as UnderstandMe runtime APIs**.

## Verified input data

- [Afutrans.Core/Jsons/languages.json](https://github.com/afrowaveltd/Afutrans/blob/main/Afutrans.Core/Jsons/languages.json) currently has 185 records with `code`, `name`, `native` (endonym), optional `rtl`. Examples: `en` English, `fr` Français, `cs` Česky, `lg` Luganda, `sw` Kiswahili, `ar` العربية.
- [Afutrans.Core/Jsons/Cldr/territoryInfo.json](https://github.com/afrowaveltd/Afutrans/blob/main/Afutrans.Core/Jsons/Cldr/territoryInfo.json) from pinned CLDR 48.2.3 provides country/language associations, approximate shares and some official-status flags. `UG` includes `en`, `sw`, `lg`, `rw`, and other codes that are **not necessarily present in the 185-item list**.
- The language-level `rtl` hint in languages.json is **not sufficient for full locale/script direction**: some languages use multiple scripts; e.g. Hausa in Latin vs Ajami script. Prefer direction based on language+script, with host overrides. A specific RTL option label does not force the full UI into RTL.

## Optional independent provider contracts

- `ILanguageCatalog` (working name): normalized locale/BCP 47 code, endonym/native display name, optional current-UI-language display name, fallback name, script, direction and provenance.
- `ILanguageTerritoryProvider` (working name): enumerate the languages associated with a country (`UG` for Uganda) with optional official status, prevalence and configurable ordering; this is discovery, **not a restriction**.
- `ILanguageAvailabilityProvider`: query which languages or source/target pairs are actually supported by a host's installed dictionary resources, an external service or LibreTranslate. Country/CLDR associations **do not imply** translator support.
- Optional `ITextDirectionResolver`: determine LTR/RTL for the actual script/locale, not just a two-letter language code, allowing override. Different visual components can use different direction while the UI overall keeps its direction.

These capabilities belong in an **optional shared Toolbox language-data provider**, not the core `ITextLocalizer` and not a hard dependency on Afutrans. A consumer may provide its own metadata provider. Dictionaryless start works when none is installed.

## Language-picker composition example

1. Host enumerates available `Locales/*.json` resource locale IDs, or the translation backend's supported language pairs, depending on the UI purpose. Do not conflate these lists.
2. For each locale, resolve the preferred **endonym** (e.g. `Français`, `Kiswahili`, `Luganda`, `العربية`) and script-aware direction. Preserve names accurately and display a fallback for unknown languages, not an empty choice.
3. If the user selects Uganda, rank/suggest Uganda-associated languages using `territoryInfo/UG`; a shortlist may prioritize widely used and official languages, configured per application.
4. **Always offer `Show all`**, because a Ugandan resident may prefer French or another language from elsewhere. A user's country is not their linguistic identity. If selecting a translation target, combine with the backend's actual available pairs; a language picker for installed UI dictionaries uses that independent capability instead.
5. Mixed RTL/LTR labels in web/native UIs need correct bidirectional isolation; web applications can use per-option language/direction information instead of flipping their entire shell.

## Data and revision caveats

- `languages.json` and CLDR supplemental coverage differ. Do not exclude Uganda minority languages merely because their native names are missing in the first file.
- CLDR country-language statistics are approximate/incomplete; not all languages spoken by immigrants/visitors appear.
- Translation engine languages are version/endpoint-specific; LibreTranslate's supported list should be retrieved via its provider adapter rather than assumed equal to any universal catalog.
- BCP 47 aliases, scripts and regions matter. Display names may require explicit correct endonyms or optional CLDR main localized display-name data not yet vendored in this repository.
- Preserve upstream provenance/license/manifest, keep any import offline-capable, and do not relocate Afutrans data until shared module and tests work.

## Planned tests

- Endonyms and RTL are read correctly for sample locale codes, with script overrides for mixed-script languages.
- Uganda recommendations include `lg`, `sw`, `en`, `rw` where present, while `Show all` preserves French and other supported languages.
- Codes found in territoryInfo but absent in languages.json receive a fallback display name and do not disappear.
- UI dictionary language list and translator-provider language pairs are independently resolved and correctly intersected/ranked where requested.
- No catalog / no internet / no CLDR does not break base localization or dictionaryless start.

**Updated implementation status:** basic reader/editor interfaces and a file-JSON language catalog with nine tests are now implemented. Territory filtering, locale/script-aware direction, remote/SQL stores, graphical/CLI editor and upstream import/merge are not implemented.

## First implemented editable JSON catalog and future upstream imports (2026-10-19)

**Actual code now present in Toolbox** (rather than only a proposed API):

- `UnderstandMe/LanguageCatalog/LanguageCatalogEntry.cs`: `Code`, `Name`, `NativeName`, nullable `RightToLeft` (legacy language-level direction hint). This deliberately does not pretend to resolve script-specific direction.
- `UnderstandMe/LanguageCatalog/ILanguageCatalogReader.cs`: `GetAll()` and `Find(code)`, independent of backend format.
- `UnderstandMe/LanguageCatalog/ILanguageCatalogEditor.cs`: separately opt-in editing with `Upsert(entry)` and explicit `Remove(code)`. Read-only backends can implement only the reader.
- `UnderstandMe/LanguageCatalog/FileJsonLanguageCatalogStore.cs`: reads/writes the **existing Afutrans `languages.json` array schema** using `System.Text.Json`. Missing file is an empty catalog; validates entries and rejects duplicate identifiers, including case-only duplicates; sorts codes on write and uses same-directory temporary file replacement. It **preserves arbitrary unknown JSON object properties** on an existing language when changing its known properties, so custom fields survive routine edits.
- Nine xUnit tests in `UnderstandMe.Tests/LanguageCatalog` cover empty startup, reading endonyms/RTL, update/add sorted preserving unknown fields, explicit remove, no-op edit, duplicate/malformed input retention, unsafe codes, and concurrent writes within one store instance.
- GitHub Actions [run 39329364298](https://github.com/afrowaveltd/Toolbox/actions/runs/39329364298) **passed** at commit `96846fac6dfc392ea14497ff0b185257a2f6eb4c`: UnderstandMe **24/24**, whole Toolbox **5938/5938**, build **0 warnings and 0 errors**. This also corrected a pre-existing invalid collection initializer in `DictionaryTextLocalizer` that had broken preceding runs.

Sample future host usage (ordinary C# code, not yet a bundled interactive editor):

```csharp
var catalog = new FileJsonLanguageCatalogStore(pathToLanguagesJson);
ILanguageCatalogReader reader = catalog;
ILanguageCatalogEditor editor = catalog;

editor.Upsert(new LanguageCatalogEntry
{
    Code = "lg",
    Name = "Ganda",
    NativeName = "Luganda"
});

var available = reader.GetAll();
bool removed = editor.Remove("lg"); // explicit action only
```

**Source options for a richer catalog:** official [Unicode CLDR language display names](https://github.com/unicode-org/cldr-json/tree/main/cldr-json/cldr-localenames-full) have more coverage than the current downloaded 185-entry seed; the [IANA Language Subtag Registry](https://www.iana.org/assignments/language-subtags-tags-extensions) is a separate authority for BCP 47 registered identifiers and preferred aliases. The CLDR `cldr-localenames-full/main/en/languages.json` provides names in **English**, *not guaranteed endonyms*. Endonyms may require each language's own locale display-name data or reviewed manual overrides; some languages are not fully covered. Keep current pinned CLDR version/provenance consistent with an explicit importer version.

### Next required capability: safe import with manual overrides

An import module must be optional, use the same reader/editor contracts or a small batch capability, and expose **preview/dry-run** counts (new/changed/unchanged/conflicts). Do not automatically replace a manually corrected `NativeName`, RTL/script override, or custom metadata with downloaded upstream values.

The current JSON adapter **edits its selected JSON file directly**. It does **not yet** have a separate override layer or upstream importer. For future refreshes, prefer e.g. a read-only versioned CLDR base plus a user-editable overlay (`languages.overrides.json`, SQL or another provider), with deterministic precedence, reviewed conflicts and explicit *tombstones* for deliberately hidden entries so imports do not silently resurrect deletions. Changes in external data should never delete local languages by omission. The editor must offer add/edit/remove/search and later a batch import preview, with access restrictions and an audit in managed/multiwriter deployments.

**Concurrency limitation:** the current JSON store synchronizes writes within **one instance**, not across processes/hosts. Shared SQL/multi-node catalogs require a revision-aware transaction/ETag provider. A graphical editor, CLI, import/merge engine, country provider, script-aware direction resolver and CLDR name import are still planned, not present.

