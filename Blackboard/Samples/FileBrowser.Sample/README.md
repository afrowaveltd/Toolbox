# Blackboard FileBrowser demonstration

A stand-alone **example application**, not part of the FileBrowser NuGet package. It demonstrates the independent Spectre.Console picker and its local filesystem source.

From Toolbox repository root:

```bash
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --file
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --directory
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --filter "*.ajis;*.json"
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --regex "\.(ajis|json)$"
dotnet run --project Blackboard/Samples/FileBrowser.Sample -- --icons-on
```

The first unlabelled argument is an optional starting folder, otherwise the current directory. Inside the picker use the arrow keys, Enter and Esc. The menu includes file filters, recursive search and directory tree navigation.

The sample does not modify files, create directories or change the process working directory. It prints the selected local path only after confirmed selection. Run it from an **interactive** terminal (including SSH); redirected stdin/stdout is not a supported input scenario.

The first release is a functional demonstration, not a guarantee of support for all terminal emulators. See [FileBrowser documentation](../../SpectreConsole/FileBrowser/README.md) and [Implementation Status](IMPLEMENTATION_STATUS.md).
