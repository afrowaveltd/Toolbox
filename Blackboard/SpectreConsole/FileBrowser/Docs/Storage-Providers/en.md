# FileBrowser 2.0: minimal, extensible storage navigation

**Status: architecture decision (2026-10-22), not implemented.** The legacy [Lutonet FileBrowser](https://github.com/Lutonet/SpectreConsoleFileBrowser) is inspiration. Preserve the small, independently usable picker and avoid importing unnecessary old code/dependencies.

## Design goal

One keyboard-first Spectre.Console file/folder picker should work with multiple *sources* without requiring a separate TUI for every protocol. Keep the default package useful on its own and tiny: **local filesystem navigation built in**, remote source adapters as **optional independently packaged NuGet extensions**. Do not require FTP dependencies to browse a local folder.

Proposed packages (IDs and directories to finalize on implementation):

- `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser`: generic navigation/rendering and local source; only Spectre.Console and essential .NET runtime dependencies.
- `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Ftp`: optional adapter supporting FTP and **FTPS**, proposed sibling directory `Blackboard/SpectreConsole/FileBrowser.Ftp` to avoid nested SDK-style project compilation pitfalls; depends on the picker contract and a maintained FTP client (candidate FluentFTP), not vice versa.
- Later separate `Sftp` adapter only if needed. **SFTP over SSH is a different protocol from FTPS over TLS.** Do not claim it is supported by the FTP adapter.
- Additional providers (network shares, S3, WebDAV, etc.) only when concrete demand warrants dependencies and maintenance.

## Small source contract (proposed public API, not yet implemented)

```csharp
public interface IFileBrowserSource
{
    string Id { get; }
    Task<IReadOnlyList<BrowserEntry>> ListAsync(BrowserLocation location, CancellationToken cancellationToken);
    Task<BrowserLocation?> GetParentAsync(BrowserLocation location, CancellationToken cancellationToken);
    Task<IReadOnlyList<BrowserLocation>> GetRootsAsync(CancellationToken cancellationToken);
}
```

`BrowserLocation` is **source-qualified** (source ID plus source-native opaque location/path). The UI must never assume all selected entries are local absolute paths. `BrowserEntry` carries stable location/identity, display name, directory/file type and optional nonsecret metadata. Selection returns the source-qualified location and a cancellation outcome. Each source owns native path semantics, enumeration and root discovery; the UI owns navigation, filtering, page size, escaping, keyboard shortcuts and localized labels.

The API should remain as small as possible; simplify or add asynchronous capabilities only when needed by concrete drivers and tests. Provide a compatibility/convenience API that returns a local filesystem absolute path for the common local-only use case. No mandatory common-framework package is justified yet.

## Platform handling

Replace fragile `Environment.OSVersion.Platform.ToString()` prefix inspection with supported .NET OS helpers (`OperatingSystem.IsWindows()` if a platform distinction is required) and platform-native `DirectoryInfo`/`Path` operations. On Windows enumerate drives with `DriveInfo.GetDrives()` where appropriate; on Unix begin at `/` and traverse mounted directories normally. Do not change `Directory.SetCurrentDirectory`: navigation state belongs to the browser/source instance. No emulated Windows drive list on Unix; no hard-coded slash conversion for remote paths.

Test Windows/Unix roots, symlinks, missing drives/mounts, permissions and relative-to-absolute conversion without trusting the process CWD after startup.

## FTP/FTPS extension boundaries

- Support listing and selecting **first**. Downloading, uploading, deleting, directory creation and remote edits are separate explicit capabilities outside the initial read-only picker. Never transfer files merely to display a listing.
- Use asynchronous network calls, cancellation, bounded timeout/retry behavior, pagination/limits where possible, recoverable connectivity/permission errors and sensible handling of server-native paths. Never block the entire TUI on a hung server.
- Plain FTP sends credentials and data insecurely. **FTPS/TLS must be the recommended default**; plain FTP can be enabled only by explicit caller configuration with appropriate warning/security policy. Verify server certificate and hostname by default: no convenient insecure bypass.
- Authentication uses references to credentials supplied by the calling application, not passwords embedded in path/URI strings, UI labels, telemetry or logs. Respect host permission policy, TLS options and cancellation.
- `FtpWebRequest` is deprecated for new development on modern .NET; choose a maintained, opt-in FTP/FTPS client dependency instead (candidate `FluentFTP`). Verify dependency API, current license and CI compatibility at the time of implementation.
- TUI localization remains independent of UnderstandMe: built-in English strings, optionally injected labels or localization callback. Errors can expose safe reason codes and localized display messages.

## Important distinction for UnderstandMe Configurator

Browsing an FTP server and choosing a **local JSON configuration file** are different workflows. `UnderstandMeSettingsJsonStore.Load(string path)` expects a local filesystem path. Do not pass remote identifiers/URLs to it or silently download/overwrite remote settings. For any future remote config editing, design a separately authorized transfer/staging workflow with remote revision/conflict checks, explicit save and recovery; `Ctrl+V` remains local draft validation and `Ctrl+S` never performs an implicit FTP upload.

## Tests and release plan

1. Implement local source plus source-qualified selection on .NET 10, with no FTP dependencies. Preserve local-only caller convenience and test old common behavior.
2. Unit tests: mocked source, different path syntaxes, root/parent navigation, local filesystem, invalid markup names, filters, cancel, platform discovery and working-directory invariance.
3. Separate FTP/FTPS adapter NuGet and tests: unavailable endpoints, authentication, TLS refusal, timeout/cancellation, non-ASCII listing, permissions, unexpected disconnect, large listings and no implicit transfer. Optional Docker-based integration tests may use an ephemeral test server; no live credentials in CI.
4. Demonstrate both base local picker and opt-in FTP plugin in future `Blackboard/Samples`; the example must never be a dependency of published packages.
5. Per-component and per-extension `README.md`, `IMPLEMENTATION_STATUS.md`, `Docs/<topic>/en.md`, English public XML docs, useful inline comments, GitHub commits and green CI before release.

The primary rule remains: **one useful component, one small package, no entire parking lot for one bicycle.**
