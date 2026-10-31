# Blackboard Samples and Showcase

This folder contains **optional, runnable demonstration executables** for Afrowave.Toolbox.Blackboard. These examples are not NuGet libraries and never become transitive dependencies of reusable controls.

## Shared showcase (recommended)

[Blackboard.Showcase](Blackboard.Showcase/README.md) is the main **extensible test application** with one menu and independently registered test modules. Current modules demonstrate FileBrowser text-file preview, JSON-only selection plus syntax-highlighted JSON, folder selection, and a four-theme Markdown gallery (Classic/Ocean/Forest/Monochrome) with images, links and nested lists.

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
```

Add new components to this single app through `IBlackboardShowcaseDemo`. Its [test suite](Blackboard.Showcase.Tests/README.md) uses a virtual Spectre console to check module dispatch, filtering and safety.

## Focused FileBrowser sample

[FileBrowser.Sample](FileBrowser.Sample/README.md) is the original minimal standalone CLI with flags for directory selection, icon modes, and custom filters. It remains useful for isolated regression testing and as a small consumer example.

```bash
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --filter "*.ajis;*.json"
```

See [Blackboard project README](../README.md) and [Implementation Status](../IMPLEMENTATION_STATUS.md).
