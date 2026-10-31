# WhenItFails.Tests

Extensive xUnit regression and contract tests for the [WhenItFails](../WhenItFails/README.md) runtime: error catalogs, safe bootstrap, validation, profiles, recovery, cancellation and public API stability.

```bash
dotnet test WhenItFails.Tests/WhenItFails.Tests.csproj -c Release
```

See [Regression coverage](Docs/Testing/en.md) and [implementation status](IMPLEMENTATION_STATUS.md). This project is **not a published NuGet library**.
