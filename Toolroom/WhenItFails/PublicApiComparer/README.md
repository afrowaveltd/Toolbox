# Public API comparer

Run the PowerShell workflow from the Toolbox root:

```powershell
& .\Toolroom\WhenItFails\PublicApiComparer\Compare-PublicApi.ps1
```

The script builds two separate temporary .NET 10 consumers: one referencing current WhenItFails source, the other restoring the exact `Afrowave.Toolbox.WhenItFails` NuGet `[0.1.0]` package. It runs matching reflection inspectors in separate processes and writes a Markdown report with DLL paths, SHA-256 hashes and public signature differences.

Pass `-Feed "<actual published feed URL or path>"` to select the published feed, and `-ReportPath "<existing-parent-directory/report.md>"` to choose the report. Without `-Feed`, configured NuGet sources and local cache are used; successful restore alone does not establish the package's publishing origin.

[Usage and limitations](Docs/Usage/en.md)
