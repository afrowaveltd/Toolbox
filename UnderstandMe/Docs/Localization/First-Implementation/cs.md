# UnderstandMe — první implementace lokalizace

**Stav:** první zdrojový kód a testovací projekt jsou uloženy v repozitáři, sestavení ani testy dosud nejsou ověřené.

**Veřejná rozhraní:** `ITextLocalizer` (`Get`, `Format`, `Reload`) a `ILocalizationDictionaryStore` (`Load`, `TryAdd`).

**Implementace:** `JsonTextLocalizer`, `FileJsonDictionaryStore`, `LocalizationOptions`.

Hotové ve zdrojovém kódu: ploché JSON slovníky, dictionaryless start, automatická registrace původní věty, jazykové fallbacky, základní pojmenované placeholdery, výslovný `Reload` a zápis přes dočasný soubor.

```csharp
ITextLocalizer t = new JsonTextLocalizer(new FileJsonDictionaryStore("Locales"));
string text = t.Format("Hello {name}",
    new Dictionary<string, string> { ["name"] = "Bolaji" }, "cs");
```

**Testování:** `dotnet test UnderstandMe.Tests/UnderstandMe.Tests.csproj`. Připraveno 14 xUnit testů; zelený výsledek zatím nepotvrzen.

**Není hotovo:** file watcher, pomocný JSON a gramatické varianty, přísnější placeholderová validace, CLDR, REST/SignalR, překladové enginy ani napojení Afutrans.

**Hranice projektů:** Dita je výhradně inspirace. Afutrans zatím beze změny, ručně upravený WebTranslator je mimo rozsah zásahů.

[Podrobný anglický popis](en.md).
