# Browse GitHub repositories by owner

**Implementation:** 2026-11-04, first public-only repository browser. Code and tests are committed; a fresh build/test result is still required.

## Commands

From the Toolbox root:

```bash
# Search for public repositories by owner:
dotnet run --project Blackboard/Tools/MdView -- afrowaveltd
dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd
dotnet run --project Blackboard/Tools/MdView -- github:afrowaveltd

# Existing direct-to-README forms still work:
dotnet run --project Blackboard/Tools/MdView -- git afrowaveltd Toolbox
dotnet run --project Blackboard/Tools/MdView -- github:afrowaveltd/Toolbox

# A local path remains explicit:
dotnet run --project Blackboard/Tools/MdView -- local README.md
```

When launched without arguments, the existing Spectre source menu's GitHub choice now accepts either `owner` or `owner/repository`. With an owner alone, `mdview` contacts GitHub's public REST API, then opens a `SelectionPrompt` of repository names and short descriptions. For 20 repositories or fewer, it also draws a compact tree under the account name; larger accounts use only the paged selection control to avoid dumping thousands of lines.

The `Filter / search by name or description` entry narrows displayed choices case-insensitively. An empty filter resets the list. `Back / cancel` or Escape exits without fetching any README. After choosing a repository, the existing preferred README source reads only that repository's README, preserving its ETag/Last-Modified and offline cache behavior.

### Name resolution rules

- A single ASCII GitHub-owner-looking argument such as `afrowaveltd` is interpreted as an owner search. Source arguments with path separators or local extensions (e.g. `README.md`) retain local meaning.
- Explicit `local PATH` always requests a local file; `git OWNER REPOSITORY` always requests the indicated README.
- Owner spelling is case-insensitive for catalog cache identity, while repository names are displayed using their GitHub-provided case.
- This feature works only in an interactive terminal: if stdout/stdin is redirected, use the full `git OWNER REPOSITORY` form instead.

## Offline, privacy and limits

The catalog uses the independent [GitHubRepositoryCatalogSource](../../../../SpectreConsole/Markdown.Cache/Docs/GitHub-Repository-Catalog/en.md). It stores a **verified public-only list**, not a copy of all repositories' README files. The catalog is reused for 30 minutes without contacting GitHub; after a failed refresh, a saved valid list can be shown with a clear stale/offline warning.

The API lists 100 repositories per page; the first implementation permits a maximum of 10 pages (1,000 repositories) and marks the output incomplete if GitHub advertises more. No token or login is requested. Large organizations beyond this cap require a future incremental browsing capability. Repository entries whose names fail validation or are marked private are excluded. Descriptions are cleaned and displayed with safe literal Spectre escaping.

A missing README in a selected repository is still possible, and will be reported normally by the existing source adapter. Selecting a repo never pushes or modifies GitHub content.

## Testing

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Blackboard/SpectreConsole/Markdown.Cache.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests.csproj
dotnet test Toolbox.sln
```

Command parsing regressions check owner-only, direct repo and explicit local forms. Virtual-terminal tests cover selection, Escape without downloading README, searching by description, literal markup-safe labels, non-interactive rejection and missing owners. Cache tests use fake HTTP handlers for user/org endpoints, pages, stale snapshots, limits and validation.

**Verification pending:** no .NET SDK is available in this execution environment. The previously reported 6,443-test Windows baseline predates this feature and the recent source-picker and highlighter changes. Run the full suite after syncing; then check `mdview git afrowaveltd` manually in Windows Terminal or Debian.

[mdview README](../../README.md) · [CLI guide](../CLI/en.md) · [Status](../../IMPLEMENTATION_STATUS.md).
