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

**Not implemented yet:** language catalog service, territory filtering, RTL resolver, upstream-data relocation or tests.
