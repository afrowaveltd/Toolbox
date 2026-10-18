# UnderstandMe — první implementace lokalizace

**Stav:** první zdrojový kód a testovací projekt jsou na GitHubu. GitHub Actions ověřily sestavení bez varování a **14/14 testů UnderstandMe + 5 928/5 928 testů celého Toolboxu**. [CI běh #39167610994](https://github.com/afrowaveltd/Toolbox/actions/runs/39167610994).

**Veřejná rozhraní:** `ITextLocalizer` (`Get`, `Format`, `Reload`) a `ILocalizationDictionaryStore` (`Load`, `TryAdd`).

**Implementace:** `JsonTextLocalizer`, `FileJsonDictionaryStore`, `LocalizationOptions`.

Hotové ve zdrojovém kódu: ploché JSON slovníky, dictionaryless start, automatická registrace původní věty, jazykové fallbacky, základní pojmenované placeholdery, výslovný `Reload` a zápis přes dočasný soubor.

```csharp
ITextLocalizer t = new JsonTextLocalizer(new FileJsonDictionaryStore("Locales"));
string text = t.Format("Hello {name}",
    new Dictionary<string, string> { ["name"] = "Bolaji" }, "cs");
```

**Testování:** `dotnet test UnderstandMe.Tests/UnderstandMe.Tests.csproj`. 14 jednotkových xUnit testů **prošlo v GitHub Actions**.

**Není hotovo:** file watcher, pomocný JSON a gramatické varianty, přísnější placeholderová validace, CLDR, REST/SignalR, překladové enginy ani napojení Afutrans.

**Hranice projektů:** Dita je výhradně inspirace. Afutrans zatím beze změny, ručně upravený WebTranslator je mimo rozsah zásahů.

[Podrobný anglický popis](en.md).
