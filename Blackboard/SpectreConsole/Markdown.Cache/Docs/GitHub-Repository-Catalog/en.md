# Public GitHub repository catalog (owner-only discovery)

**Status:** implemented in-repository on 2026-11-04; new tests and full solution build have not yet been verified.

## Purpose and API contracts

The `GitHubRepositoryCatalogSource` adapter lists public GitHub repositories when a reader knows an account but not the repository name. For example, `mdview git afrowaveltd` allows selecting a project before fetching its preferred README. This adapter returns typed repository metadata; it does not parse or render Markdown, download README content, execute code, authenticate, or write to a remote repository.

It calls the public GitHub REST API:

1. `GET https://api.github.com/users/{owner}` to distinguish a `User` from an `Organization`.
2. For organizations: `GET /orgs/{owner}/repos?per_page=100&page=N&type=public`.
3. For users: `GET /users/{owner}/repos?per_page=100&page=N&type=owner`.
4. The adapter reads `Link: ... rel="next"` but builds the next URL itself, never following arbitrary links supplied by a server.

These endpoints allow anonymous reads of public metadata. GitHub normally limits **unauthenticated REST traffic to 60 requests per hour per IP**, so a local reuse policy matters. Public-only results are enforced even if the caller accidentally injects an authenticated HTTP client: entries with `private: true` are skipped.

## Persistent cache and offline behavior

A complete, validated catalog is serialized to JSON and published atomically through the existing `VerifiedDocumentCache` using a separate case-insensitive owner key (`github-repositories:v1:{owner}`). It cannot collide with existing `github-readme:{owner}/{repository}` entries. This is a small repository index, not a cache of every README.

- The index is considered fresh for **30 minutes**. Fresh verified content is served locally without any HTTP request.
- After 30 minutes, a refresh queries the owner and then paginates repositories. The previous index stays intact until *all requested pages* are successfully parsed and the new index passes cache validation.
- Network failures, rate limiting, invalid JSON, unacceptable redirects and body-size violations use a previously verified index as explicitly **stale** data when available.
- With no valid cached index, failures return an explicit error, not an invented empty list.
- Each response body has a **4 MiB** cap. One refresh makes at most **11 requests** (one profile plus ten catalog pages), displaying at most **1,000 repositories**. If GitHub says more exist, the result has `Truncated=true` and an explicit warning; the first 1,000 are not misrepresented as exhaustive.

Repository labels are validated, descriptions are scrubbed of control characters and truncated, and the UI escapes all Spectre markup. No credentials, query tokens or private repos are persisted.

## Public C# API

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
var cache = new VerifiedDocumentCache(CacheDirectory.GetDefaultPath());
var catalog = new GitHubRepositoryCatalogSource(http, cache);
var result = await catalog.ListAsync("afrowaveltd");

foreach (var repository in result.Repositories)
    Console.WriteLine(repository.Name);

// result.State: Downloaded / FreshCache / Stale / Error.
// result.Warning: an offline, incomplete or failed-refresh explanation.
```

A nullable `TimeProvider` constructor parameter makes expiry tests deterministic. The catalog source intentionally does not depend on Spectre.Console. Presentation and interactive filtering live in `MdViewGitHubRepositoryPicker`.

## Tests and limitations

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Cache.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests.csproj
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

Fake-HTTP tests cover user vs organization endpoints, pagination and duplicate/private filtering, caching and case-insensitive owner keys, offline stale fallback after expiry, malformed JSON, invalid owners, 404 and the 10-page limit. No live GitHub traffic is required for these tests.

The repository catalog is intentionally **not** a recursive source-tree browser; it selects repositories (not directories or branches inside a repo). A repository without a README will still return an appropriate error from the existing `GitHubReadmeSource`. The current `mdview` Markdown renderer remains whole-document, in-memory, not a streaming pager.

Official API references: [List user repositories](https://docs.github.com/en/rest/repos/repos#list-repositories-for-a-user), [List organization repositories](https://docs.github.com/en/rest/repos/repos#list-organization-repositories), and [REST rate limits](https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api).

[Cache README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md) · [mdview owner discovery](../../../../Tools/MdView/Docs/GitHub-Owner-Discovery/en.md).
