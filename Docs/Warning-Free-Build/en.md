# Warning-free builds and XML API documentation

**Updated:** 2026-11-07. This page records the compiler/analyzer cleanup following a maintainer-confirmed green test suite. It does not claim that any unexecuted build was warning-free.

## Source of evidence

A GitHub Actions Linux build (run [41846625891](https://github.com/afrowaveltd/Toolbox/actions/runs/41846625891), 2026-11-04) completed successfully but emitted these distinct diagnostics, repeated across compilation invocations:

- **CS1591**: missing XML documentation on public members, concentrated in `Blackboard/SpectreConsole/Markdown.Cache` and `Blackboard/Tools/MdView/MdViewCommand.cs`.
- **CS1573**: seven missing positional-constructor `<param>` elements on `CachedDocument`, which had only its `ContentPath` parameter documented.
- **xUnit2013**: two collection-count assertions in `ExchangeHouse/Markdown.Tests/MarkdownParserTests.cs`, which should use `Assert.Single`.
- A GitHub Actions Node 20 deprecation warning from `actions/checkout@v4` and `actions/setup-dotnet@v4`; these steps now use **v5** (Node 24).

The project still deliberately generates XML docs for non-test assemblies via `Directory.Build.props`. **No warning IDs were disabled or suppressed to accomplish this cleanup.**

## Documentation contract

Every publicly visible API should have accurate English XML documentation. Use `<summary>` for public types, enum values and members; `<param>` for every parameter on a documented API (including positional-record parameters); `<returns>` for meaningful return values; and `<exception cref="...">` when the failure contract is important.

The new documentation describes actual cache, source selection, freshness, error and CLI parsing behavior rather than adding placeholder summaries solely to silence diagnostics. Positional records may require a complete set of `<param>` tags once *any* positional parameter is documented; otherwise CS1573 is emitted.

Separately, prefer analyzer-recommended test idioms (`Assert.Single` instead of `Assert.Equal(1, collection.Count)`) without weakening the assertions.

## Rebuild and check

From the current repository root on a machine with the .NET SDK:

```powershell
dotnet restore Toolbox.sln
dotnet build Toolbox.sln --no-incremental --no-restore
dotnet test Toolbox.sln --no-restore
python Toolroom/Documentation/Check-Markdown-Links.py
```

Read the warnings from a **fresh/non-incremental** build; an incremental `dotnet test` may appear clean because unchanged projects are not recompiled. GitHub Actions performs a fresh checkout and a build on Linux and a targeted UI build on Windows.

As of this documentation update, the code and docs fixes were committed, and post-change CI verification should be consulted before declaring the build warning-free. A passing test suite alone proves tests, not warning-free compilation.

### Messages unrelated to XML comments

- **NETSDK1057** indicates a developer is running a preview .NET SDK. It is an informational SDK message, not a missing XML comment. Use a supported stable SDK when appropriate; do not suppress compiler documentation checking to hide it.
- Git's `init.defaultBranch` message is a Git hint from workflow checkout, not a C# compiler warning.
- Workflow action runtime deprecation is solved by maintaining the workflow actions, not by modifying .NET XML comments.

## Maintenance

Update the owning project's `IMPLEMENTATION_STATUS.md` after any new public API or change to the build diagnostics. Avoid `<NoWarn>` or `#pragma warning disable CS1591` as substitutes for documenting supported APIs. Document intentionally public contracts, and only narrow their accessibility when it is a genuine API design decision backed by tests.

[Toolbox README](../../README.md) · [Root implementation status](../../IMPLEMENTATION_STATUS.md) · [Development and verification](../Development-and-Verification/en.md).
