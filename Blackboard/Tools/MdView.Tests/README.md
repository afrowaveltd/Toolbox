# mdview CLI tests

xUnit tests for command grammar and the independent Spectre.Console interactive picker. The picker tests use `TestConsole` with simulated keyboard input and temporary local Markdown files. No real network is used.

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
```

The original ten command-grammar tests passed in the user-supplied 6,443-green Windows baseline. Eleven new interactive picker cases cover Escape/back behavior, picking real Markdown, GitHub and URL entry, dangerous URL rejection, terminal capability and filename validation. The **new** tests are not yet executed/CI confirmed. Real-terminal visual verification and full command-to-render integration coverage remain pending.

[CLI implementation](../MdView/README.md).
