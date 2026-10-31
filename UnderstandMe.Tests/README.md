# UnderstandMe.Tests

The independent xUnit test project for Afrowave.Toolbox.UnderstandMe (.NET 10). It exercises localization, placeholder validation, memory/JSON dictionary behavior, settings and additional UnderstandMe core components without requiring the Afutrans translation service.

## Run

```bash
dotnet test UnderstandMe.Tests/UnderstandMe.Tests.csproj
```

For the dictionary concurrency regression seen on Windows in 2026-10-29:

```bash
dotnet test UnderstandMe.Tests/UnderstandMe.Tests.csproj --filter "FullyQualifiedName~DictionaryTextLocalizerTests"
```

The same focused suite runs automatically on the GitHub Windows runner. See [Localization regression testing](Docs/Localization-Regression/en.md) and [implementation status](IMPLEMENTATION_STATUS.md). Tests do not guarantee persistence on a permanently read-only or unavailable disk.
