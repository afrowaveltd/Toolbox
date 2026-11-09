# GitHub repository Markdown files (2026-11-09)

## Purpose

`GitHubRepositoryFileSource` extends the existing independent
`Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache` project
without changing `GitHubReadmeSource`. It retrieves a **specific
Markdown file at an explicitly resolved repository ref**, using official
GitHub REST APIs and the existing `VerifiedDocumentCache`.

The source itself performs no rendering, link parsing or terminal I/O.

## Source identity discovery

For a public repository, `ResolveReadmeAsync(owner, repository)` first
calls:

1. `GET https://api.github.com/repos/{owner}/{repository}` to obtain
   `default_branch` from GitHub's actual repository metadata.
2. `GET https://api.github.com/repos/{owner}/{repository}/readme?ref={escaped-ref}`
   using the explicitly selected ref to obtain the actual README `path`.

Both responses use `Accept: application/vnd.github+json`, are
schema/identity-validated before being cached, and may reuse the
previously validated metadata when offline. GitHub's README JSON may
include its entire Base64-encoded content: responses are bounded to
**2 MiB**, but only the validated branch/path projection (maximum
**8 KiB**) is persisted with the original HTTP validator, avoiding
duplicate large metadata cache files. When either metadata
request falls back to a cached copy, the returned
`GitHubReadmeLocation.State` is `Stale` and its `Warning` explains
the condition. The ref and README filename are never guessed.

```csharp
var source = new GitHubRepositoryFileSource(httpClient, verifiedCache);
var readme = await source.ResolveReadmeAsync("afrowaveltd", "Toolbox");
var result = await source.OpenAsync(readme.File);
```

## Exact-ref file download and cache

`OpenAsync(GitHubRepositoryFile)` requests the official API:

```text
GET /repos/{owner}/{repository}/contents/{escaped-path}?ref={escaped-ref}
Accept: application/vnd.github.raw+json
```

Every path segment is escaped independently; slashes in branch names are
escaped in the ref query. Owner/repository keys are compared
case-insensitively; ref and path are **case-sensitive** and independently
encoded in the cache key. The provider rejects invalid repository
identifiers, disallowed file types, path traversal, encoded separators,
query/fragment injections, unsupported filename extensions and unsafe
ref characters before issuing a request.

The cache retains the same strict UTF-8, content-hash, terminal-control,
bounded-write, conditional ETag/Last-Modified and offline fallback
contracts as its other document adapters. GitHub API metadata is capped
at 64 KiB; document size is controlled by the caller's
`VerifiedDocumentCache` limit. An HTTP 304 updates only a verified
matching entry; a failed request returns `Stale` only when a validated
entry for the exact source exists.

This source uses an injected `HttpClient`; callers **must disable
automatic redirects**, as the `mdview` CLI does. Responses with a
changed final request URI are rejected, but allowing redirects on the
caller client can already have contacted an unintended destination.

## Current limitations

This is a public-repository Markdown reader: it does not authenticate,
support private repositories, download non-Markdown attachments, execute
remote content, or translate arbitrary GitHub website/blob URLs.
`browse-git` is limited to one repository and its initially resolved
ref; automatic cross-repository changes are disallowed.

Unverified or stale metadata is **not** represented as fresh, but when
offline the previously verified ref may no longer match GitHub's
current default branch. GitHub API request limits still apply.

## Verification

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Cache.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests.csproj
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

Regression tests cover large README JSON projection, real branch/README
identity discovery, exact-ref
requests with slash-containing refs, 304 revalidation, offline/stale
fallback, invalid metadata and traversal rejection, cache isolation by
ref, and failed UTF-8 replacement without cache corruption. The tests
for this increment have not yet been confirmed green by the maintainer.
