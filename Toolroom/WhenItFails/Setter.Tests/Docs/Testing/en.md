# Setter CLI regression tests

The Setter test project uses xUnit to check catalog authoring commands, output formatting, validation, error paths, safe file operations and automation behavior. The executable owns command dispatch; WhenItFails owns catalog models and validation.

From Toolbox root run `dotnet test Toolroom/WhenItFails/Setter.Tests/Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.csproj -c Release`. Detailed usage and release documentation remain in the sibling [Setter README](../../Setter/README.md).
