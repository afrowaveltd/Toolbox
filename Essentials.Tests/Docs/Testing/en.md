# Essentials tests and contract stability

The project tests Essentials results/responses, diagnostics, metadata, extensions, guards and value objects. Run it independently with `dotnet test Essentials.Tests/Essentials.Tests.csproj -c Release` or through `Toolbox.sln`. If changing an Essentials API, preserve source compatibility deliberately and extend the corresponding tests.

The production package is [Essentials](../../../Essentials/README.md); tests are not shipped as a standalone NuGet library.
