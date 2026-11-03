# mdview CLI tests

Focused xUnit command parser tests for no-argument menu selection, help, positional local/URL/GitHub syntax, case-insensitive verbs and malformed calls.

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
```

These tests have **not** been executed in this environment. Command dispatch, actual terminal rendering and network error handling still need end-to-end integration coverage after .NET CI is green.

[CLI implementation](../MdView/README.md).
