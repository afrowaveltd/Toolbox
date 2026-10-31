# WhenItFails regression coverage

Tests cover catalog providers, configuration and bootstrap boundaries, runtime validation, public API contracts, cancellation, failure recovery and preservation of project files. They must not be replaced by a simple successful-build check.

Run `dotnet test WhenItFails.Tests/WhenItFails.Tests.csproj -c Release`. Release candidate/API comparison scripts are separate under `Toolroom/WhenItFails`. For detailed historical checkpoints consult the owning library's [implementation status](../../WhenItFails/IMPLEMENTATION_STATUS.md).
