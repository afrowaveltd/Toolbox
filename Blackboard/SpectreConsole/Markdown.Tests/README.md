# Blackboard.SpectreConsole.Markdown.Tests

Virtual-terminal tests for the independent themeable Markdown renderer, using xUnit and Spectre.Console.Testing.

Run:

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Tests.csproj
```

Covers headings, lists, inline text, fenced-code isolation, link destinations, themes, terminal-control filtering and source immutability. Real terminal smoke and advanced CommonMark coverage are out of scope. See [Testing](Docs/Testing/en.md) and [Implementation status](IMPLEMENTATION_STATUS.md).
