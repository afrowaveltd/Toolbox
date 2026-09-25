# Public API comparer

Run the PowerShell workflow from the Toolbox root:

```powershell
& .\Toolroom\WhenItFails\PublicApiComparer\Compare-PublicApi.ps1
```

The script builds two separate temporary .NET 10 consumers: one referencing current WhenItFails source, the other restoring the exact `Afrowave.Toolbox.WhenItFails` NuGet `[0.1.0]` package. It runs matching reflection inspectors in separate processes and writes a Markdown report with DLL paths, SHA-256 hashes and public signature differences.

Pass `-Feed "<actual published feed URL or path>"` to select the published feed, and `-ReportPath "<existing-parent-directory/report.md>"` to choose the report. Without `-Feed`, configured NuGet sources and local cache are used; successful restore alone does not establish the package's publishing origin.

[Usage and limitations](Docs/Usage/en.md)

## Precompiled consumer binary smoke

Run `Test-PublishedConsumerBinary.ps1` from the Toolbox root to compile a disposable consumer against exactly requested package `[0.1.0]`, then run that same executable unchanged after replacing **only** its WhenItFails.dll with the current source build. For an opt-in second scenario, pass `-ExerciseInitialization` to activate bundled defaults and check the historical `UNKNOWNERROR` descriptor by name, ID and code in both runs, without rebuilding the consumer. For the isolated project catalog path, pass `-ExerciseProjectInitialization` instead: the same precompiled consumer is run with separate temp workspaces, five file-preservation hashes, and project-activation descriptor checks. The bundled and project initialization flags are mutually exclusive; both modes have been confirmed PASS. Add `-ExerciseProjectRecovery` only together with the project initialization flag to test malformed JSON and retained previous context; that recovery mode is now maintainer-confirmed PASS. The separate `-ExerciseFirstStartFallback` switch checks a malformed project catalog before any active context exists, without changing user files; this first-start fallback mode was confirmed PASS. See [binary smoke instructions](../../../WhenItFails/Docs/Published-Binary-Consumer-Smoke/en.md). This tests one legacy store/DI/runtime path; it is not an exhaustive ABI or behavioral compatibility guarantee. All eight current smoke modes are maintainer-confirmed PASS, including `-ExerciseCancelledActivation`, which exercises pre-cancelled initialization and reset with the unchanged 0.1.0 consumer. Last library suite **1445/1445 GREEN**.
